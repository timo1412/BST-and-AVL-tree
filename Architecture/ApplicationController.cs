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
        public IReadOnlyList<PCRTest> ListAllByDistrictFromGui(
    DateTime? dateFrom, string timeFrom,
    DateTime? dateTo, string timeTo,
    string districtText)
        {
            // 1) Validácia vstupov
            if (dateFrom is null) throw new InvalidOperationException("Zvoľ počiatočný dátum.");
            if (dateTo is null) throw new InvalidOperationException("Zvoľ koncový dátum.");

            var fromTs = ParseTime(timeFrom);   // existujúci helper
            var toTs = ParseTime(timeTo);

            var from = dateFrom.Value.Date + fromTs;
            var to = dateTo.Value.Date + toTs;

            if (to < from)
                throw new InvalidOperationException("Dátum/čas 'Do' musí byť ≥ 'Od'.");

            // kód okresu 1..79
            int district = ParseIntInRange(districtText, 1, 79, "Kód okresu musí byť v intervale 1..79.");

            // 2) Delegácia na View → Model
            var (ok, error, tests) = _view.ListAllByDistrictPeriod(from, to, district);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Výstup pre DataGrid
            return tests ?? Array.Empty<PCRTest>();
        }

        public IReadOnlyList<PCRTest> ListPositiveByRegionFromGui(DateTime? dateFrom, string timeFrom,DateTime? dateTo, string timeTo, string regionText)
        {
            // 1) Validácia vstupov
            if (dateFrom is null) throw new InvalidOperationException("Zvoľ počiatočný dátum.");
            if (dateTo is null) throw new InvalidOperationException("Zvoľ koncový dátum.");

            var fromTs = ParseTime(timeFrom);
            var toTs = ParseTime(timeTo);

            var from = dateFrom.Value.Date + fromTs;
            var to = dateTo.Value.Date + toTs;

            if (to < from)
                throw new InvalidOperationException("Dátum/čas 'Do' musí byť ≥ 'Od'.");

            // kód kraja 1..8
            int region = ParseIntInRange(regionText, 1, 8, "Kód kraja musí byť v intervale 1..8.");

            // 2) Delegácia na View → Model
            var (ok, error, tests) = _view.ListPositiveByRegionPeriod(from, to, region);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Výsledok pre DataGrid
            return tests ?? Array.Empty<PCRTest>();
        }
        public IReadOnlyList<PCRTest> ListPositiveAllFromGui(DateTime? dateFrom, string timeFrom, DateTime? dateTo, string timeTo)
        {
            // 1) Validácia vstupov
            if (dateFrom is null) throw new InvalidOperationException("Zvoľ počiatočný dátum.");
            if (dateTo is null) throw new InvalidOperationException("Zvoľ koncový dátum.");

            var fromTs = ParseTime(timeFrom);
            var toTs = ParseTime(timeTo);

            var from = dateFrom.Value.Date + fromTs;
            var to = dateTo.Value.Date + toTs;

            if (to < from)
                throw new InvalidOperationException("Dátum/čas 'Do' musí byť ≥ 'Od'.");

            // 2) Delegácia na View → Model
            var (ok, error, tests) = _view.ListPositiveAllPeriod(from, to);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Výsledok pre DataGrid
            return tests ?? Array.Empty<PCRTest>();
        }
        public IReadOnlyList<PCRTest> ListAllByRegionFromGui(DateTime? dateFrom, string timeFrom,DateTime? dateTo, string timeTo, string regionText)
        {
            // 1) Validácia vstupov
            if (dateFrom is null) throw new InvalidOperationException("Zvoľ počiatočný dátum.");
            if (dateTo is null) throw new InvalidOperationException("Zvoľ koncový dátum.");

            var fromTs = ParseTime(timeFrom);
            var toTs = ParseTime(timeTo);

            var from = dateFrom.Value.Date + fromTs;
            var to = dateTo.Value.Date + toTs;

            if (to < from)
                throw new InvalidOperationException("Dátum/čas 'Do' musí byť ≥ 'Od'.");

            // 2) Kód kraja 1..8
            int region = ParseIntInRange(regionText, 1, 8, "Kód kraja musí byť v intervale 1..8.");

            // 3) Delegácia na View → Model
            var (ok, error, tests) = _view.ListAllByRegionPeriod(from, to, region);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 4) Výsledok pre DataGrid (prípadne utriediť podľa dátumu, ak to nerobí model)
            return tests ?? Array.Empty<PCRTest>();
        }
        public IReadOnlyList<PCRTest> ListAllFromGui(DateTime? dateFrom, string timeFrom,DateTime? dateTo, string timeTo)
        {
            // 1) Validácia vstupov
            if (dateFrom is null) throw new InvalidOperationException("Zvoľ počiatočný dátum.");
            if (dateTo is null) throw new InvalidOperationException("Zvoľ koncový dátum.");

            var fromTs = ParseTime(timeFrom);
            var toTs = ParseTime(timeTo);

            var from = dateFrom.Value.Date + fromTs;
            var to = dateTo.Value.Date + toTs;

            if (to < from)
                throw new InvalidOperationException("Dátum/čas 'Do' musí byť ≥ 'Od'.");

            // 2) View → Model
            var (ok, error, tests) = _view.ListAllPeriod(from, to);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Výstup pre DataGrid (prípadne zoradenie podľa dátumu)
            return (tests ?? Array.Empty<PCRTest>())
                .OrderBy(t => t.DateStartTest)
                .ToList();
        }
        public IReadOnlyList<Person> ListSickByDistrictAtDateFromGui(DateTime? atDate, string atTime,string districtText,string xDaysText)
        {
            // 1) Validácia a parsovanie vstupov
            if (atDate is null) throw new InvalidOperationException("Zvoľ dátum.");

            var atTs = ParseTime(atTime); // používa existujúci helper HH:mm
            var at = atDate.Value.Date + atTs;

            // kód okresu 1..79
            int district = ParseIntInRange(districtText, 1, 79, "Kód okresu musí byť v intervale 1..79.");

            // X dní: povoľme 1..365 (prípadne uprav podľa zadania)
            int xDays = ParseIntInRange(xDaysText, 1, 365, "X dní musí byť v intervale 1..365.");

            // 2) Delegácia na View → Model
            var (ok, error, persons) = _view.ListSickByDistrictAtDate(at, district, xDays);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Výsledok pre tabuľku „Osoby“ (prípadné zoradenie ak chceš)
            return persons ?? Array.Empty<Person>();
        }
        public IReadOnlyList<(Person Person, PCRTest Test)> ListSickByDistrictAtDateSortedWithTestFromGui(
            DateTime? atDate, string atTime,
            string districtText,
            string xDaysText)
        {
            // 1) Validácia a parsovanie
            if (atDate is null) throw new InvalidOperationException("Zvoľ dátum.");
            var atTs = ParseTime(atTime);                 // existujúci helper HH:mm
            var at = atDate.Value.Date + atTs;

            int district = ParseIntInRange(districtText, 1, 79, "Kód okresu musí byť v intervale 1..79.");
            int xDays = ParseIntInRange(xDaysText, 1, 365, "X dní musí byť v intervale 1..365.");

            // 2) View → Model: potrebujeme osoby aj ich „referenčný“ pozitívny test
            var (ok, error, pairs) = _view.ListSickByDistrictAtDateWithTest(at, district, xDays);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Zoradenie podľa hodnoty testu (desc), pri zhode podľa času testu (desc)
            return (pairs ?? Array.Empty<(Person Person, PCRTest Test)>())
                .OrderByDescending(p => p.Test.ValueOfTest)
                .ThenByDescending(p => p.Test.DateStartTest)
                .ToList();
        }
        public Person AddPersonFromGui(string firstName, string lastName, DateTime? birthDate)
        {
            // 1) Validácia vstupov
            if (string.IsNullOrWhiteSpace(firstName))
                throw new InvalidOperationException("Zadaj krstné meno.");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new InvalidOperationException("Zadaj priezvisko.");

            if (birthDate is null)
                throw new InvalidOperationException("Zvoľ dátum narodenia.");

            var birth = birthDate.Value.Date;
            var fn = firstName.Trim();
            var ln = lastName.Trim();

            // 2) Vytvorme rozumnú náhodnú váhu (rovnaká logika ako v seedovaní)
            double weight = Math.Round(_rnd.NextDouble() * 100.0, 2, MidpointRounding.AwayFromZero);

            // 3) View → Model
            var (ok, error, person) = _view.AddPerson(fn, ln, birth, weight);
            if (!ok) throw new InvalidOperationException(error ?? "Vloženie osoby zlyhalo.");

            return person!;
        }
        public PCRTest FindPcrByCodeFromGui(string pcrCodeText)
        {
            if (!int.TryParse(pcrCodeText?.Trim(), out var code))
                throw new InvalidOperationException("Kód PCR testu musí byť celé číslo.");

            var (ok, error, test) = _view.FindPcrByCode(code);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadanie zlyhalo.");

            return test!;
        }

        public PCRTest DeletePcrByCodeFromGui(string pcrCodeText)
        {
            if (!int.TryParse(pcrCodeText?.Trim(), out var code))
                throw new InvalidOperationException("Kód PCR testu musí byť celé číslo.");

            var (ok, error, deleted) = _view.DeletePcrByCode(code);
            if (!ok) throw new InvalidOperationException(error ?? "Mazanie zlyhalo.");

            return deleted!;
        }
        public bool DeletePersonWithTestsFromGui(string personIdText)
        {
            if (string.IsNullOrWhiteSpace(personIdText))
                throw new InvalidOperationException("Zadaj unikátne ID/rodné číslo pacienta.");

            var personId = personIdText.Trim();

            // View → Model: trvalé vymazanie osoby aj všetkých jej PCR testov
            var (ok, _error) = _view.DeletePersonWithTests(personId);

            // Ak chceš pri chybe zobraziť detailnú hlášku (a máš globálny try/catch v GUI),
            // odkomentuj:
            // if (!ok) throw new InvalidOperationException(_error ?? "Mazanie osoby zlyhalo.");

            return ok;
        }
        public IReadOnlyList<PCRTest> ListAllByWorkplaceFromGui(DateTime? dateFrom, string timeFrom,DateTime? dateTo, string timeTo,string workCodeText)
        {
            if (dateFrom is null) throw new InvalidOperationException("Zvoľ počiatočný dátum.");
            if (dateTo is null) throw new InvalidOperationException("Zvoľ koncový dátum.");

            var fromTs = ParseTime(timeFrom);
            var toTs = ParseTime(timeTo);

            var from = dateFrom.Value.Date + fromTs;
            var to = dateTo.Value.Date + toTs;

            if (to < from)
                throw new InvalidOperationException("Dátum/čas 'Do' musí byť ≥ 'Od'.");

            // kód pracoviska: kladné celé číslo
            int workCode = ParseIntInRange(workCodeText, 1, int.MaxValue, "Kód pracoviska musí byť kladné celé číslo.");

            var (ok, error, tests) = _view.ListAllByWorkplacePeriod(from, to, workCode);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            return tests ?? Array.Empty<PCRTest>();
        }
        public IReadOnlyList<(int Region, int SickCount)> ListRegionsBySickCountAtDateFromGui(DateTime? atDate, string atTime, string xDaysText)
        {
            // 1) Validácia a parsovanie vstupov
            if (atDate is null) throw new InvalidOperationException("Zvoľ dátum.");

            var atTs = ParseTime(atTime); // existujúci helper HH:mm
            var at = atDate.Value.Date + atTs;

            // X dní povoľme napr. 1..365
            int xDays = ParseIntInRange(xDaysText, 1, 365, "X dní musí byť v intervale 1..365.");

            // 2) View → Model: vráť zoznam (regionCode, sickCount) k času 'at'
            var (ok, error, rows) = _view.ListRegionsBySickCountAtDate(at, xDays);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Zoradenie podľa počtu chorých (desc), pri zhode podľa kódu kraja (asc)
            return (rows ?? Array.Empty<(int Region, int SickCount)>())
                .OrderByDescending(r => r.SickCount)
                .ThenBy(r => r.Region)
                .ToList();
        }
        public IReadOnlyList<(int District, int SickCount)> ListDistrictsBySickCountAtDateFromGui(DateTime? atDate, string atTime, string xDaysText)
        {
            // 1) Validácia a parsovanie vstupov
            if (atDate is null) throw new InvalidOperationException("Zvoľ dátum.");

            var atTs = ParseTime(atTime); // HH:mm helper, ktorý už máš
            var at = atDate.Value.Date + atTs;

            // X dní povoľme napr. 1..365
            int xDays = ParseIntInRange(xDaysText, 1, 365, "X dní musí byť v intervale 1..365.");

            // 2) View → Model: vráť zoznam (districtCode, sickCount) k času 'at'
            var (ok, error, rows) = _view.ListDistrictsBySickCountAtDate(at, xDays);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Zoradenie podľa počtu chorých (desc), pri zhode podľa kódu okresu (asc)
            return (rows ?? Array.Empty<(int District, int SickCount)>())
                .OrderByDescending(r => r.SickCount)
                .ThenBy(r => r.District)
                .ToList();
        }
        public IReadOnlyList<Person> ListSickByRegionAtDateFromGui(DateTime? atDate, string atTime, string regionText,string xDaysText)
        {
            // 1) Validácia a parsovanie vstupov
            if (atDate is null) throw new InvalidOperationException("Zvoľ dátum.");

            var atTs = ParseTime(atTime);                 // HH:mm helper
            var at = atDate.Value.Date + atTs;

            int region = ParseIntInRange(regionText, 1, 8, "Kód kraja musí byť v intervale 1..8.");
            int xDays = ParseIntInRange(xDaysText, 1, 365, "X dní musí byť v intervale 1..365.");

            // 2) View → Model
            var (ok, error, persons) = _view.ListSickByRegionAtDate(at, region, xDays);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Výsledok pre tabuľku „Osoby“
            return persons ?? Array.Empty<Person>();
        }
        public IReadOnlyList<Person> ListSickAllAtDateFromGui(DateTime? atDate, string atTime, string xDaysText)
        {
            // 1) Validácia a parsovanie vstupov
            if (atDate is null) throw new InvalidOperationException("Zvoľ dátum.");

            var atTs = ParseTime(atTime);                   // existujúci helper HH:mm
            var at = atDate.Value.Date + atTs;
            int xDays = ParseIntInRange(xDaysText, 1, 365, "X dní musí byť v intervale 1..365.");

            // 2) View → Model
            var (ok, error, persons) = _view.ListSickAllAtDate(at, xDays);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Výsledok pre tabuľku „Osoby“
            return persons ?? Array.Empty<Person>();
        }
        public IReadOnlyList<(Person Person, PCRTest Test)> ListTopSickPerDistrictAtDateFromGui(DateTime? atDate, string atTime, string xDaysText)
        {
            // 1) Validácia a parsovanie vstupov
            if (atDate is null) throw new InvalidOperationException("Zvoľ dátum.");

            var atTs = ParseTime(atTime);                   // existujúci helper HH:mm
            var at = atDate.Value.Date + atTs;
            int xDays = ParseIntInRange(xDaysText, 1, 365, "X dní musí byť v intervale 1..365.");

            // 2) View → Model: potrebujeme všetkých „chorých“ s ich pozitívnym testom k času 'at'
            //    (napr. posledný relevantný pozitívny test v okne X dní).
            //    Očakávaný návrat: kolekcia párov (Person, PCRTest).
            var (ok, error, pairs) = _view.ListSickAllAtDateWithTest(at, xDays);
            if (!ok) throw new InvalidOperationException(error ?? "Vyhľadávanie zlyhalo.");

            // 3) Vyber 1 najvyššiu hodnotu testu z každého okresu.
            //    Pri zhode hodnoty testu uprednostni novší test; potom stabilne podľa ID osoby.
            var topPerDistrict = (pairs ?? Array.Empty<(Person Person, PCRTest Test)>())
                .GroupBy(p => p.Test.NumberOfDistrict)
                .Select(g => g
                    .OrderByDescending(x => x.Test.ValueOfTest)
                    .ThenByDescending(x => x.Test.DateStartTest)
                    .ThenBy(x => x.Person.UniqueNumber)
                    .First())
                .OrderBy(x => x.Test.NumberOfDistrict)
                .ToList();

            return topPerDistrict;
        }
        public string SaveAllToFileInAppRoot()
        {
            var (ok, error, path) = _view.SaveAllToFileInAppRoot();
            if (!ok) throw new InvalidOperationException(error ?? "Ukladanie zlyhalo.");
            return path!;
        }
        public (int persons, int tests) LoadAllFromFolderFromGui(string folderPath, bool clearExisting = true)
        {
            var (ok, error, result) = _view.LoadAllFromFolder(folderPath, clearExisting);
            if (!ok) throw new InvalidOperationException(error ?? "Načítanie dát zo zložky zlyhalo.");
            return result!.Value; // (persons, tests)
        }

    }
}
