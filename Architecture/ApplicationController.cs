using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SemestralnaPracaAUS2.Architecture.ApplicationView;

namespace SemestralnaPracaAUS2.Architecture
{
    public sealed class ApplicationController
    {
        private readonly ApplicationView _view;
        private readonly Random _rnd = new Random();

        public ApplicationController(ApplicationView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void InsertPcrFromGui(
            DateTime? selectedDate,   // dpPcrDate.SelectedDate
            string timeText,          // tbPcrTime.Text (HH:mm)
            string districtText,      // tbDistrictCode.Text
            string regionText,        // tbRegionCode.Text
            bool resultPositive,      // chkResultPositive.IsChecked == true
            string valueText,         // tbTestValue.Text
            string note               // tbNote.Text
        )
        {
            // 1) Validácia + parsovanie vstupov
            if (selectedDate is null)
                throw new InvalidOperationException("Zvoľ dátum testu.");

            var time = ParseTime(timeText); // HH:mm
            var dt = selectedDate.Value.Date + time;

            int district = ParseIntInRange(districtText, 1, 79, "Kód okresu musí byť v intervale 1..79.");
            int region = ParseIntInRange(regionText, 1, 8, "Kód kraja musí byť v intervale 1..8.");

            double value = ParseDouble(valueText, "Hodnota testu musí byť číslo.");

            // 3) Zostavenie DTO a volanie View→Model
            var dto = new PcrInputDto(
                DateStartTest: dt,
                NumberOfDistrict: district,
                NumberOfRegion: region,
                ResultOfTest: resultPositive,
                ValueOfTest: Math.Round(value, 2, MidpointRounding.AwayFromZero),
                Note: string.IsNullOrWhiteSpace(note) ? "NOTE" : note.Trim()
            );

            var (ok, error) = _view.InsertPcr(dto);
            if (!ok) throw new InvalidOperationException(error ?? "Neznáma chyba pri vkladaní PCR.");
        }

        // ---------- Helpers ----------
        private static TimeSpan ParseTime(string hhmm)
        {
            if (string.IsNullOrWhiteSpace(hhmm))
                throw new InvalidOperationException("Zadaj čas vo formáte HH:mm (napr. 14:30).");

            if (!TimeSpan.TryParseExact(hhmm.Trim(), @"hh\:mm", CultureInfo.CurrentCulture, out var ts))
                if (!TimeSpan.TryParseExact(hhmm.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out ts))
                    throw new InvalidOperationException("Čas musí byť vo formáte HH:mm (napr. 14:30).");

            // voliteľné: obmedz na 06:00–20:00
            var min = new TimeSpan(6, 0, 0);
            var max = new TimeSpan(20, 0, 0);
            if (ts < min || ts > max)
                throw new InvalidOperationException("Čas musí byť medzi 06:00 a 20:00.");
            return ts;
        }

        private static int ParseIntInRange(string text, int min, int max, string rangeError)
        {
            if (!int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var n))
                throw new InvalidOperationException("Zadaná hodnota musí byť celé číslo.");
            if (n < min || n > max) throw new InvalidOperationException(rangeError);
            return n;
        }

        private static double ParseDouble(string text, string error)
        {
            // podporíme slovenskú čiarku aj bodku
            if (double.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out var d))
                return d;
            if (double.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                return d;
            throw new InvalidOperationException(error);
        }
    }
}
