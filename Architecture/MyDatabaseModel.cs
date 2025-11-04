using SemestralnaPracaAUS2.Structures;
using SemestralnaPracaAUS2.TestData;
using SemestralnaPracaAUS2.Wrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SemestralnaPracaAUS2.TestData.SeedContracts;

namespace SemestralnaPracaAUS2.Model
{
    public sealed class MyDatabaseModel
    {
        private readonly AVLTree<PersonByUniqueNumber> _idxPersonById = new AVLTree<PersonByUniqueNumber>();
        private readonly AVLTree<PersonByBirth> _idxPersonByBirth = new AVLTree<PersonByBirth>();
        private readonly AVLTree<PcrByCode> _idxByCode = new AVLTree<PcrByCode>();
        private readonly AVLTree<PcrByPersonDate> _idxByPerson = new AVLTree<PcrByPersonDate>();
        private readonly AVLTree<PcrByDate> _idxByDate = new AVLTree<PcrByDate>();
        private readonly AVLTree<PcrByRegionDate> _idxByRegionDate = new AVLTree<PcrByRegionDate>();
        private readonly AVLTree<PcrByDistrictDate> _idxByDistrictDate = new AVLTree<PcrByDistrictDate>();
        private readonly AVLTree<PcrByWorkplaceDate> _idxByWorkplaceDate = new AVLTree<PcrByWorkplaceDate>();

        private readonly Random _rnd = new Random();
        private readonly object _sync = new object();
        private const string PersonAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public MyDatabaseModel() 
        {
        
        }
        public void InsertPcr(PCRTest t)
        {
            if (t is null) throw new ArgumentNullException(nameof(t));
            _idxByCode.Add(PcrByCode.Of(t));
            _idxByPerson.Add(PcrByPersonDate.Of(t));
            _idxByDate.Add(PcrByDate.Of(t));
            _idxByRegionDate.Add(PcrByRegionDate.Of(t));
            _idxByDistrictDate.Add(PcrByDistrictDate.Of(t));
            _idxByWorkplaceDate.Add(PcrByWorkplaceDate.Of(t));
        }

        public SeedResult SeedRandom(SeedRequest req)
        {
            if (req.Persons <= 0) throw new ArgumentOutOfRangeException(nameof(req.Persons));
            if (req.PcrPerPerson < 0) throw new ArgumentOutOfRangeException(nameof(req.PcrPerPerson));
            if (req.DateTo < req.DateFrom) throw new ArgumentException("DateTo must be ≥ DateFrom.");
            if (req.DayTimeTo <= req.DayTimeFrom) throw new ArgumentException("DayTimeTo must be > DayTimeFrom.");
            if (req.PositiveRatio is < 0.0 or > 1.0) throw new ArgumentOutOfRangeException(nameof(req.PositiveRatio));

            int personsInserted = 0;
            int pcrInserted = 0;

            for (int i = 0; i < req.Persons; i++)
            {
                // tu si môžeš vybrať logiku mien / váh / dátumov: ja generujem náhodne z modelu
                var firstName = $"Name{i + 1}";
                var lastName = $"Surname{i + 1}";

                // náhodná váha a dátum narodenia v rozmedzí (môžeš upraviť)
                double weight;
                DateTime birth;
                lock (_sync)
                {
                    weight = Math.Round(_rnd.NextDouble() * 100.0, 2, MidpointRounding.AwayFromZero);
                    DateTime minDate = new DateTime(1900, 1, 1);
                    DateTime maxDate = new DateTime(2025, 12, 31);
                    var totalDays = (maxDate - minDate).Days;
                    birth = minDate.AddDays(_rnd.Next(totalDays + 1));
                }

                var person = CreateAndAddPerson(firstName, lastName, weight, birth);
                personsInserted++;

                // vytvoríme pre osobu pcr testy
                for (int k = 0; k < req.PcrPerPerson; k++)
                {
                    // generovanie dátumu a času v rámci parametrov
                    DateTime dt;
                    lock (_sync)
                    {
                        var totalDays = (req.DateTo.Date - req.DateFrom.Date).Days;
                        var dayOffset = totalDays == 0 ? 0 : _rnd.Next(totalDays + 1);
                        var day = req.DateFrom.Date.AddDays(dayOffset);
                        var totalMinutes = (int)(req.DayTimeTo - req.DayTimeFrom).TotalMinutes;
                        var minuteOffset = totalMinutes == 0 ? 0 : _rnd.Next(totalMinutes + 1);
                        dt = day + req.DayTimeFrom + TimeSpan.FromMinutes(minuteOffset);
                    }

                    bool result;
                    lock (_sync) { result = _rnd.NextDouble() < req.PositiveRatio; }
                    int district, region;
                    lock (_sync) { district = _rnd.Next(1, 80); region = _rnd.Next(1, 9); }
                    double value;
                    lock (_sync) { value = Math.Round(_rnd.NextDouble() * 1000.0, 2, MidpointRounding.AwayFromZero); }

                    CreateAndAddPcrForPerson(
                        personUniqueNumber: person.UniqueNumber,
                        dateStartTest: dt,
                        numberOfDistrict: district,
                        numberOfRegion: region,
                        resultOfTest: result,
                        valueOfTest: value,
                        note: "SEED"
                    );

                    pcrInserted++;
                }
            }
            return new SeedResult(PersonsInserted: personsInserted, PcrInserted: pcrInserted);
        }
        public PCRTest CreateAndAddPcrForPerson(
            string personUniqueNumber,
            DateTime dateStartTest,
            int numberOfDistrict,
            int numberOfRegion,
            bool resultOfTest,
            double valueOfTest,
            string note)
        {
            if (string.IsNullOrWhiteSpace(personUniqueNumber)) throw new ArgumentNullException(nameof(personUniqueNumber));

            // 1) over, že osoba existuje
            if (!_idxPersonById.Find(PersonByUniqueNumber.FromKey(personUniqueNumber), out _))
                throw new InvalidOperationException($"Pacient s ID '{personUniqueNumber}' neexistuje.");

            // 2) vygeneruj unikátne uniqueNumberPCR
            var uniquePcr = GenerateUniquePcrNumber();

            // 3) vytvor PCRTest cez parametricky ctor (ten, čo sme pridali)
            var pcr = new PCRTest(
                dateStartTest: dateStartTest,
                numberOfDistrict: numberOfDistrict,
                numberOfRegion: numberOfRegion,
                resultOfTest: resultOfTest,
                valueOfTest: valueOfTest,
                note: note,
                uniqueNumberPerson: personUniqueNumber,
                uniqueNumberPcr: uniquePcr,
                uniqueNumberPcrPlace: null // alebo nechaj model vygenerovať ho náhodne v ctor, prípadne generuj tu podobne ako PCRPlace
            );

            // 4) indexuj do všetkých indexov
            _idxByCode.Add(PcrByCode.Of(pcr));
            _idxByPerson.Add(PcrByPersonDate.Of(pcr));
            _idxByDate.Add(PcrByDate.Of(pcr));
            _idxByRegionDate.Add(PcrByRegionDate.Of(pcr));
            _idxByDistrictDate.Add(PcrByDistrictDate.Of(pcr));
            Debug.WriteLine(pcr.NumberOfDistrict);
            _idxByWorkplaceDate.Add(PcrByWorkplaceDate.Of(pcr));

            return pcr;
        }
        public Person CreateAndAddPerson(string firstName, string lastName, double weight, DateTime birth)
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentNullException(nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentNullException(nameof(lastName));

            // vygenerujeme unikátne UniqueNumber pred vytvorením objektu
            var uniqueNumber = GenerateUniquePersonId();

            var p = new Person(firstName, lastName, weight, birth, uniqueNumber);

            // pridanie do indexov (ak AVLTree.Add vyhodí chybu pri duplicate, tak sa to tu už nestane)
            _idxPersonById.Add(PersonByUniqueNumber.Of(p));
            _idxPersonByBirth.Add(PersonByBirth.Of(p));

            return p;
        }
        private int GenerateUniquePcrNumber()
        {
            lock (_sync)
            {
                while (true)
                {
                    int code = _rnd.Next(1, 1_000_000 + 1);
                    if (!_idxByCode.Find(PcrByCode.FromKey(code), out _))
                        return code;
                }
            }
        }
        public (Person person, PCRTest pcr) FindPcrByCodeForPerson(string personUniqueNumber, int pcrCode)
        {
            if (string.IsNullOrWhiteSpace(personUniqueNumber))
                throw new ArgumentNullException(nameof(personUniqueNumber));
            if (pcrCode <= 0) throw new ArgumentOutOfRangeException(nameof(pcrCode));

            // 1) nájdi osobu
            if (!_idxPersonById.Find(PersonByUniqueNumber.FromKey(personUniqueNumber), out var personWrap))
                throw new InvalidOperationException($"Pacient s ID '{personUniqueNumber}' neexistuje.");

            var person = personWrap.Value; //?? throw new InvalidOperationException($"Pacient s ID '{personUniqueNumber}' neexistuje (sentinel).");

            // 2) nájdi PCR podľa kódu
            if (!_idxByCode.Find(PcrByCode.FromKey(pcrCode), out var pcrWrap))
                throw new InvalidOperationException($"PCR test s kódom '{pcrCode}' neexistuje.");

            var pcr = pcrWrap.Value; // uprav názov property podľa tvojich wrapperov

            // 3) skontroluj, že PCR patrí tejto osobe
            if (!string.Equals(pcr.UniqueNumberPerson, person.UniqueNumber, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"PCR test s kódom '{pcrCode}' nepatrí pacientovi s ID '{personUniqueNumber}'."
                );

            return (person, pcr);
        }
        private string GenerateUniquePersonId(int length = 6)
        {
            lock (_sync)
            {
                while (true)
                {
                    var chars = new char[length];
                    for (int i = 0; i < length; i++)
                        chars[i] = PersonAlphabet[_rnd.Next(PersonAlphabet.Length)];
                    var id = new string(chars);

                    // overenie unikátnosti v indexe
                    if (!_idxPersonById.Find(PersonByUniqueNumber.FromKey(id), out _))
                        return id;
                    // inak pokus znova
                }
            }
        }
        public IReadOnlyList<PCRTest> ListPcrForPerson(string personId)
        {
            if (string.IsNullOrWhiteSpace(personId))
                throw new ArgumentNullException(nameof(personId));

            // 1) Over, že pacient existuje
            if (!_idxPersonById.Find(PersonByUniqueNumber.FromKey(personId), out _))
                throw new InvalidOperationException($"Pacient s ID '{personId}' neexistuje.");

            // 2) Vytvor kľúče pre interval (od najmenšieho po najväčší dátum)
            var lo = PcrByPersonDate.Low(personId, DateTime.MinValue);
            var hi = PcrByPersonDate.High(personId, DateTime.MaxValue);

            var results = new List<PCRTest>();

            // 3) Prejdi interval v AVL indexe (osoba + dátum)
            foreach (var wrap in _idxByPerson.Range(lo, hi))
            {
                var t = wrap.Value; // PCRTest
                                    // bezpečnostný check na osobu (index by to mal garantovať)
                if (t.UniqueNumberPerson == personId)
                    results.Add(t);
            }

            // 4) Pre istotu zotriedime podľa dátumu (Range už býva in-order)
            results.Sort((a, b) => a.DateStartTest.CompareTo(b.DateStartTest));

            return results;
        }
        public IReadOnlyList<PCRTest> ListPositiveByDistrictPeriod(DateTime from, DateTime to, int district)
        {
            if (district <= 0) throw new ArgumentOutOfRangeException(nameof(district));
            if (to < from) (from, to) = (to, from);

            // kľúče pre interval (okres + dátum)
            var lo = PcrByDistrictDate.Low(district, from);
            var hi = PcrByDistrictDate.High(district, to);

            var results = new List<PCRTest>();

            // Použitie existujúcej metódy AVL: Range(low, high)
            foreach (var wrap in _idxByDistrictDate.Range(lo, hi))
            {
                var t = wrap.Value;          // wrapper nesie PCRTest
                if (t.ResultOfTest           // len pozitívne
                    && t.NumberOfDistrict == district) // istota na okres
                {
                    results.Add(t);
                }
            }

            // deterministické zoradenie podľa dátumu
            results.Sort((a, b) => a.DateStartTest.CompareTo(b.DateStartTest));
            return results;
        }

        public IReadOnlyList<PCRTest> ListAllByDistrictPeriod(DateTime from, DateTime to, int district)
        {
            if (district <= 0) throw new ArgumentOutOfRangeException(nameof(district));
            if (to < from) (from, to) = (to, from);

            var lo = PcrByDistrictDate.Low(district, from);
            var hi = PcrByDistrictDate.High(district, to);

            var results = new List<PCRTest>();

            // Použijeme existujúcu intervalovú enumeráciu AVL stromu
            foreach (var wrap in _idxByDistrictDate.Range(lo, hi))
            {
                var t = wrap.Value; // PCRTest
                if (t.NumberOfDistrict == district) // istota na okres
                    results.Add(t);
            }

            // zoradenie podľa dátumu a času vykonania
            results.Sort((a, b) => a.DateStartTest.CompareTo(b.DateStartTest));
            return results;
        }
    }
}
