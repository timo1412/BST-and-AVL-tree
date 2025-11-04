using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SemestralnaPracaAUS2.Architecture.ApplicationView;
using static SemestralnaPracaAUS2.TestData.SeedContracts;

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

        public (Person person, PCRTest pcr) FindPcrForPersonFromGui(string personIdText, string pcrCodeText)
        {
            if (string.IsNullOrWhiteSpace(personIdText))
                throw new InvalidOperationException("Zadaj unikátne ID pacienta.");

            if (!int.TryParse(pcrCodeText?.Trim(), out var pcrCode))
                throw new InvalidOperationException("Kód PCR testu musí byť celé číslo.");

            var (ok, error, result) = _view.FindPcrForPerson(personIdText.Trim(), pcrCode);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadanie zlyhalo.");

            // result určite nie je null ak ok==true
            return (result!.Value.Person, result.Value.Pcr);
        }

        public void InsertPcrFromGui(DateTime? selectedDate, string timeText, string districtText, string regionText, bool resultPositive, string valueText,string note)
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

        public IReadOnlyList<PCRTest> ListPositiveByDistrictFromGui(DateTime? dateFrom, string timeFrom,DateTime? dateTo, string timeTo,string districtText)
        {
            // 1) Validácia vstupov
            if (dateFrom is null) throw new InvalidOperationException("Zvoľ počiatočný dátum.");
            if (dateTo is null) throw new InvalidOperationException("Zvoľ koncový dátum.");

            var fromTs = ParseTime(timeFrom);   // používa existujúci helper
            var toTs = ParseTime(timeTo);

            var from = dateFrom.Value.Date + fromTs;
            var to = dateTo.Value.Date + toTs;

            if (to < from)
                throw new InvalidOperationException("Dátum/čas 'Do' musí byť ≥ 'Od'.");

            // kód okresu 1..79 (rovnaká logika ako pri Insert)
            int district = ParseIntInRange(districtText, 1, 79, "Kód okresu musí byť v intervale 1..79.");

            // 2) Delegácia na View → Model
            var (ok, error, tests) = _view.ListPositiveByDistrictPeriod(from, to, district);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Vraciame kolekciu pre DataGrid
            return tests ?? Array.Empty<PCRTest>();
        }


        public void SeedSystemFromGui(
            int persons,
            int pcrPerPerson,
            DateTime dateFrom,
            DateTime dateTo,
            string dayTimeFromText,   // "06:00"
            string dayTimeToText,     // "20:00"
            double positiveRatio      // 0.60 = 60:40
        )
        {
            if (persons <= 0) throw new InvalidOperationException("Počet osôb musí byť > 0.");
            if (pcrPerPerson < 0) throw new InvalidOperationException("Počet testov na osobu nesmie byť záporný.");
            if (dateTo < dateFrom) throw new InvalidOperationException("Dátum 'Do' musí byť ≥ 'Od'.");

            var fromTs = ParseTime(dayTimeFromText);
            var toTs = ParseTime(dayTimeToText);
            if (toTs <= fromTs) throw new InvalidOperationException("Denný interval musí mať tvar Od < Do.");
            if (positiveRatio is < 0.0 or > 1.0) throw new InvalidOperationException("PositiveRatio musí byť v intervale 0..1.");

            var req = new SeedRequest(
                Persons: persons,
                PcrPerPerson: pcrPerPerson,
                DateFrom: dateFrom,
                DateTo: dateTo,
                DayTimeFrom: fromTs,
                DayTimeTo: toTs,
                PositiveRatio: positiveRatio
            );

            var (ok, error, result) = _view.SeedRandom(req);
            if (!ok) throw new InvalidOperationException(error ?? "Seed sa nepodaril.");

            // Voliteľne informuj GUI (MessageBox / status bar)
            MessageBox.Show(
                $"Seed hotový.\nOsoby: {result?.PersonsInserted}\nPCR: {result?.PcrInserted}",
                "Seed",
                MessageBoxButton.OK, MessageBoxImage.Information
            );
        }

        public IReadOnlyList<PCRTest> ListPcrForPersonFromGui(string personIdText)
        {
            if (string.IsNullOrWhiteSpace(personIdText))
                throw new InvalidOperationException("Zadaj unikátne ID pacienta.");

            var personId = personIdText.Trim();

            // View vracia PCR testy pre osobu; model to vie rýchlo cez index PcrByPersonDate
            var (ok, error, tests) = _view.ListPcrForPerson(personId);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // Garantuj poradie podľa dátumu/času (ak by to už View/Model nerobili)
            return (tests ?? Array.Empty<PCRTest>())
                .OrderBy(t => t.DateStartTest)
                .ToList();
        }
    }
}
