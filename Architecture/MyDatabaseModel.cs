using SemestralnaPracaAUS2.Structures;
using SemestralnaPracaAUS2.TestData;
using SemestralnaPracaAUS2.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static SemestralnaPracaAUS2.TestData.SeedContracts;

namespace SemestralnaPracaAUS2.Architecture
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

        private readonly AVLTree<SickByDistrictDate> _idxSickByDistrictDate = new AVLTree<SickByDistrictDate>();

        private readonly AVLTree<PcrPosByDate> _posByDate = new();
        private readonly AVLTree<PcrPosByRegionDate> _posByRegionDate = new();
        private readonly AVLTree<PcrPosByDistrictDate> _posByDistrictDate = new();

        private readonly Random _rnd = new Random();
        private readonly object _sync = new object();
        private const string PersonAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public MyDatabaseModel() 
        {
        
        }
        public void InsertPcr(PCRTest t)
        {
            if (t is null) throw new ArgumentNullException(nameof(t));

            if (!string.IsNullOrWhiteSpace(t.UniqueNumberPerson))
            {
                if (!_idxPersonById.Find(PersonByUniqueNumber.FromKey(t.UniqueNumberPerson), out var pWrap) ||
                    pWrap.Value is null)
                {
                    throw new InvalidOperationException(
                        $"Osoba s ID '{t.UniqueNumberPerson}' neexistuje – test nemožno priradiť.");
                }
                var person = pWrap.Value;
                person.personPcrTests.Add(PcrByCode.Of(t));

                if (t.ResultOfTest)
                {
                    UpdateSickIndexForPositive(person, t);
                }
            }

            _idxByCode.Add(PcrByCode.Of(t));
            _idxByPerson.Add(PcrByPersonDate.Of(t));
            _idxByDate.Add(PcrByDate.Of(t));
            _idxByRegionDate.Add(PcrByRegionDate.Of(t));
            _idxByDistrictDate.Add(PcrByDistrictDate.Of(t));
            _idxByWorkplaceDate.Add(PcrByWorkplaceDate.Of(t));
            if (t.ResultOfTest)
            {
                _posByDistrictDate.Add(PcrPosByDistrictDate.Of(t));
                _posByRegionDate.Add(PcrPosByRegionDate.Of(t));
            }
        }
        public PCRTest UpdatePcr(int code, int region, int district, bool result, double value, string note)
        {
            if (code <= 0)
                throw new ArgumentOutOfRangeException(nameof(code), "Kód PCR testu musí byť kladné celé číslo.");
            if (region < 1 || region > 8)
                throw new ArgumentOutOfRangeException(nameof(region), "Kód kraja musí byť v intervale 1..8.");
            if (district < 1 || district > 79)
                throw new ArgumentOutOfRangeException(nameof(district), "Kód okresu musí byť v intervale 1..79.");

            // 1) Nájdeme existujúci test podľa kódu
            if (!_idxByCode.Find(PcrByCode.FromKey(code), out var wrap) || wrap.Value is null)
                throw new InvalidOperationException($"PCR test s kódom {code} neexistuje.");

            var old = wrap.Value;

            // 2) Zistíme vlastníka (ak existuje)
            Person? owner = null;
            if (!string.IsNullOrEmpty(old.UniqueNumberPerson) &&
                _idxPersonById.Find(PersonByUniqueNumber.FromKey(old.UniqueNumberPerson), out var ownerWrap) &&
                ownerWrap.Value is Person p)
            {
                owner = p;
            }

            // 3) Vytvoríme NOVÚ inštanciu PCRTest podľa tvojho konštruktora
            var updated = new PCRTest(
                dateStartTest: old.DateStartTest,
                numberOfDistrict: district,
                numberOfRegion: region,
                resultOfTest: result,
                valueOfTest: value,
                note: string.IsNullOrWhiteSpace(note) ? old.Note : note.Trim(),
                uniqueNumberPerson: old.UniqueNumberPerson,
                uniqueNumberPcr: old.UniqueNumberPCR,
                uniqueNumberPcrPlace: old.UniqueNumberPCRPlace
            );

            // 4) Aktualizácia osobného stromu vlastníka
            if (owner is not null)
            {
                owner.personPcrTests.Delete(PcrByCode.FromKey(old.UniqueNumberPCR));
                owner.personPcrTests.Add(PcrByCode.Of(updated));
            }

            // 5) Aktualizácia všetkých globálnych indexov (remove -> add)
            _idxByCode.Delete(PcrByCode.FromKey(old.UniqueNumberPCR));
            _idxByCode.Add(PcrByCode.Of(updated));

            _idxByPerson.Delete(PcrByPersonDate.Of(old));
            _idxByPerson.Add(PcrByPersonDate.Of(updated));

            _idxByDate.Delete(PcrByDate.Of(old));
            _idxByDate.Add(PcrByDate.Of(updated));

            _idxByRegionDate.Delete(PcrByRegionDate.Of(old));
            _idxByRegionDate.Add(PcrByRegionDate.Of(updated));

            _idxByDistrictDate.Delete(PcrByDistrictDate.Of(old));
            _idxByDistrictDate.Add(PcrByDistrictDate.Of(updated));

            _idxByWorkplaceDate.Delete(PcrByWorkplaceDate.Of(old));
            _idxByWorkplaceDate.Add(PcrByWorkplaceDate.Of(updated));
            if (old.ResultOfTest) 
            {
                _posByDistrictDate.Delete(PcrPosByDistrictDate.Of(old));
                _posByRegionDate.Delete(PcrPosByRegionDate.Of(old));
            }


            if (updated.ResultOfTest) 
            {
                _posByDistrictDate.Add(PcrPosByDistrictDate.Of(updated));
                _posByRegionDate.Add(PcrPosByRegionDate.Of(updated));
            }
                
            return updated;
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
            if (string.IsNullOrWhiteSpace(personUniqueNumber))
                throw new ArgumentNullException(nameof(personUniqueNumber));

            // 1) over, že osoba existuje
            if (!_idxPersonById.Find(PersonByUniqueNumber.FromKey(personUniqueNumber), out var personWrap) ||
                personWrap.Value is null)
                throw new InvalidOperationException($"Pacient s ID '{personUniqueNumber}' neexistuje.");

            var person = personWrap.Value;

            // 2) vygeneruj unikátne uniqueNumberPCR
            var uniquePcr = GenerateUniquePcrNumber();

            // 3) vytvor PCRTest s vlastníkom
            var pcr = new PCRTest(
                dateStartTest: dateStartTest,
                numberOfDistrict: numberOfDistrict,
                numberOfRegion: numberOfRegion,
                resultOfTest: resultOfTest,
                valueOfTest: valueOfTest,
                note: note,
                uniqueNumberPerson: personUniqueNumber,
                uniqueNumberPcr: uniquePcr,
                uniqueNumberPcrPlace: null
            );

            // 4) zanes do globálnych indexov
            _idxByCode.Add(PcrByCode.Of(pcr));
            _idxByPerson.Add(PcrByPersonDate.Of(pcr));
            _idxByDate.Add(PcrByDate.Of(pcr));
            _idxByRegionDate.Add(PcrByRegionDate.Of(pcr));
            _idxByDistrictDate.Add(PcrByDistrictDate.Of(pcr));
            _idxByWorkplaceDate.Add(PcrByWorkplaceDate.Of(pcr));
            if (pcr.ResultOfTest) 
            {
                _posByDistrictDate.Add(PcrPosByDistrictDate.Of(pcr));
                _posByRegionDate.Add(PcrPosByRegionDate.Of(pcr));
                UpdateSickIndexForPositive(person, pcr);
            }
                
            // 5) zanes aj do osobného stromu danej osoby
            person.personPcrTests.Add(PcrByCode.Of(pcr));

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

            var lo = PcrPosByDistrictDate.Low(district, from);
            var hi = PcrPosByDistrictDate.High(district, to);

            return _posByDistrictDate
                .Range(lo, hi)
                .Select(w => w.Value!) 
                .ToList();
        }

        public IReadOnlyList<PCRTest> ListAllByDistrictPeriod(DateTime from, DateTime to, int district)
        {
            if (district <= 0) throw new ArgumentOutOfRangeException(nameof(district));
            if (to < from) (from, to) = (to, from);

            var lo = PcrByDistrictDate.Low(district, from);
            var hi = PcrByDistrictDate.High(district, to);

            return _idxByDistrictDate
                .Range(lo, hi)
                .Select(w => w.Value!)   // unwrap na PCRTest
                .ToList();
        }
        public IReadOnlyList<PCRTest> ListPositiveByRegionPeriod(DateTime from, DateTime to, int region)
        {
            if (region <= 0) throw new ArgumentOutOfRangeException(nameof(region));
            if (to < from) (from, to) = (to, from);

            var lo = PcrPosByRegionDate.Low(region, from);
            var hi = PcrPosByRegionDate.High(region, to);

            // Range vracia len pozitívne testy daného kraja a už v zoradení podľa (Date, Code)
            return _posByRegionDate
                .Range(lo, hi)
                .Select(w => w.Value!)
                .ToList();
        }
        public IReadOnlyList<PCRTest> ListPositiveAllPeriod(DateTime from, DateTime to)
        {
            if (to < from) (from, to) = (to, from);

            var lo = PcrPosByDate.Low(from);
            var hi = PcrPosByDate.High(to);

            // Range vracia len pozitívne testy daného obdobia, už zoradené podľa (Date, Code)
            return _posByDate
                .Range(lo, hi)
                .Select(w => w.Value!)
                .ToList();
        }
        public IReadOnlyList<PCRTest> ListAllByRegionPeriod(DateTime from, DateTime to, int region)
        {
            if (region <= 0) throw new ArgumentOutOfRangeException(nameof(region));
            if (to < from) (from, to) = (to, from);

            var lo = PcrByRegionDate.Low(region, from);
            var hi = PcrByRegionDate.High(region, to);

            var results = new List<PCRTest>();

            // použijeme existujúcu intervalovú enumeráciu AVL stromu
            foreach (var wrap in _idxByRegionDate.Range(lo, hi))
            {
                var t = wrap.Value; 
                if (t.NumberOfRegion == region)   
                    results.Add(t);               
            }

            // deterministické poradie podľa času vykonania
            results.Sort((a, b) => a.DateStartTest.CompareTo(b.DateStartTest));
            return results;
        }
        public IReadOnlyList<PCRTest> ListAllPeriod(DateTime from, DateTime to)
        {
            if (to < from) (from, to) = (to, from);

            var lo = PcrByDate.Low(from);
            var hi = PcrByDate.High(to);

            var results = new List<PCRTest>();

            // prejdeme celý interval v AVL indexe podľa dátumu
            foreach (var wrap in _idxByDate.Range(lo, hi))
            {
                var t = wrap.Value; // PCRTest
                results.Add(t);     // všetky testy (pozitívne aj negatívne)
            }

            // deterministické poradie podľa času vykonania
            results.Sort((a, b) => a.DateStartTest.CompareTo(b.DateStartTest));
            return results;
        }
        public IReadOnlyList<Person> ListSickByDistrictAtDate(DateTime at, int district, int xDays)
        {
            if (district <= 0) throw new ArgumentOutOfRangeException(nameof(district));
            if (xDays <= 0) throw new ArgumentOutOfRangeException(nameof(xDays));

            var atDate = at.Date;                         
            var from = atDate.AddDays(-(xDays - 1)); 

            var lo = SickByDistrictDate.Low(district, from);
            var hi = SickByDistrictDate.High(district, atDate);

            var persons = new List<Person>();

            foreach (var wrap in _idxSickByDistrictDate.Range(lo, hi))
            {
                if (wrap.Value != null)
                    persons.Add(wrap.Value);
            }

            persons.Sort((a, b) =>
            {
                int c = string.Compare(a.LastName, b.LastName, StringComparison.Ordinal);
                if (c != 0) return c;
                c = string.Compare(a.FirstName, b.FirstName, StringComparison.Ordinal);
                if (c != 0) return c;
                return string.Compare(a.UniqueNumber, b.UniqueNumber, StringComparison.Ordinal);
            });

            return persons;
        }
        public IReadOnlyList<(Person Person, PCRTest Test)> ListSickByDistrictAtDateWithTest(DateTime at, int district, int xDays)
        {
            if (district <= 0) throw new ArgumentOutOfRangeException(nameof(district));
            if (xDays <= 0) throw new ArgumentOutOfRangeException(nameof(xDays));

            var from = at.AddDays(-(xDays - 1));

            var lo = PcrByDistrictDate.Low(district, from);
            var hi = PcrByDistrictDate.High(district, at);

            // pre každú osobu uchovávame najnovší pozitívny test v okne
            var latestPositiveByPerson = new Dictionary<string, PCRTest>(StringComparer.Ordinal);

            foreach (var wrap in _idxByDistrictDate.Range(lo, hi))
            {
                var t = wrap.Value; // PCRTest
                if (!t.ResultOfTest) continue;                 // len pozitívne
                if (t.NumberOfDistrict != district) continue;  // istota na okres

                var pid = t.UniqueNumberPerson;

                if (!latestPositiveByPerson.TryGetValue(pid, out var curr) || t.DateStartTest > curr.DateStartTest)
                    latestPositiveByPerson[pid] = t;
            }

            var pairs = new List<(Person Person, PCRTest Test)>(latestPositiveByPerson.Count);

            foreach (var (personId, test) in latestPositiveByPerson)
            {
                if (_idxPersonById.Find(PersonByUniqueNumber.FromKey(personId), out var pWrap) && pWrap.Value != null)
                {
                    pairs.Add((pWrap.Value, test));
                }
                // ak by osoba chýbala (nemalo by sa stať), preskočíme
            }

            // zoradenie podľa hodnoty testu (najväčšia najskôr), pri zhode novší test skôr, potom podľa ID
            pairs.Sort((a, b) =>
            {
                int c = b.Test.ValueOfTest.CompareTo(a.Test.ValueOfTest);
                if (c != 0) return c;
                c = b.Test.DateStartTest.CompareTo(a.Test.DateStartTest);
                if (c != 0) return c;
                return string.Compare(a.Person.UniqueNumber, b.Person.UniqueNumber, StringComparison.Ordinal);
            });
            return pairs;
        }
        public Person AddPerson(string firstName, string lastName, DateTime birthDate, double weight)
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentNullException(nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentNullException(nameof(lastName));
            if (birthDate == default) throw new ArgumentOutOfRangeException(nameof(birthDate));
            if (double.IsNaN(weight) || double.IsInfinity(weight)) throw new ArgumentOutOfRangeException(nameof(weight));
            return CreateAndAddPerson(firstName, lastName, weight, birthDate);
        }
        public PCRTest FindPcrByCode(int code)
        {
            if (code <= 0) throw new ArgumentOutOfRangeException(nameof(code));

            if (!_idxByCode.Find(PcrByCode.FromKey(code), out var wrap))
                throw new InvalidOperationException($"PCR test s kódom '{code}' neexistuje.");

            var test = wrap.Value;
            if (test == null)
                throw new InvalidOperationException($"PCR test s kódom '{code}' je neplatný (empty wrapper).");

            return test;
        }
        public PCRTest DeletePcrByCode(int code)
        {
            if (code <= 0) throw new ArgumentOutOfRangeException(nameof(code));

            // 1) nájdi test podľa kódu
            if (!_idxByCode.Find(PcrByCode.FromKey(code), out var byCodeWrap))
                throw new InvalidOperationException($"PCR test s kódom '{code}' neexistuje.");

            var test = byCodeWrap.Value
                ?? throw new InvalidOperationException($"Nájdený wrapper pre kód '{code}' je prázdny.");

            // 2) odstráň zo všetkých indexov
            bool ok = true;
            ok &= _idxByCode.Delete(PcrByCode.Of(test));
            ok &= _idxByPerson.Delete(PcrByPersonDate.Of(test));
            ok &= _idxByDate.Delete(PcrByDate.Of(test));
            ok &= _idxByRegionDate.Delete(PcrByRegionDate.Of(test));
            ok &= _idxByDistrictDate.Delete(PcrByDistrictDate.Of(test));
            ok &= _idxByWorkplaceDate.Delete(PcrByWorkplaceDate.Of(test));
            if (test.ResultOfTest) 
            {
                ok &= _posByDistrictDate.Delete(PcrPosByDistrictDate.Of(test));
                ok &= _posByRegionDate.Delete(PcrPosByRegionDate.Of(test));
            }
                ok &= _posByDistrictDate.Delete(PcrPosByDistrictDate.Of(test));
            _posByRegionDate.Delete(PcrPosByRegionDate.Of(test));
            if (!ok)
                throw new InvalidOperationException("Nepodarilo sa odstrániť test zo všetkých indexov (nekonzistentný stav).");

            return test;
        }
        public void DeletePersonWithTests(string personId)
        {
            if (string.IsNullOrWhiteSpace(personId))
                throw new ArgumentNullException(nameof(personId));

            // 1) Over, že osoba existuje a načítaj jej záznam (kvôli zmazaniu z person indexov)
            if (!_idxPersonById.Find(PersonByUniqueNumber.FromKey(personId), out var personWrap) || personWrap.Value == null)
                throw new InvalidOperationException($"Pacient s ID '{personId}' neexistuje.");

            var person = personWrap.Value;

            // 2) Získaj všetky PCR testy tejto osoby cez index (person + date)
            var lo = PcrByPersonDate.Low(personId, DateTime.MinValue);
            var hi = PcrByPersonDate.High(personId, DateTime.MaxValue);

            // POZOR: nezmazávaj počas iterácie priamo z toho istého indexu,
            // najprv si testy nazbieraj do listu.
            var testsToDelete = new List<PCRTest>();
            foreach (var w in _idxByPerson.Range(lo, hi))
            {
                if (w.Value != null && w.Value.UniqueNumberPerson == personId)
                    testsToDelete.Add(w.Value);
            }

            // 3) Zmaž všetky testy zo všetkých PCR indexov
            foreach (var t in testsToDelete)
            {
                bool ok = true;
                ok &= _idxByCode.Delete(PcrByCode.Of(t));
                ok &= _idxByPerson.Delete(PcrByPersonDate.Of(t));
                ok &= _idxByDate.Delete(PcrByDate.Of(t));
                ok &= _idxByRegionDate.Delete(PcrByRegionDate.Of(t));
                ok &= _idxByDistrictDate.Delete(PcrByDistrictDate.Of(t));
                ok &= _idxByWorkplaceDate.Delete(PcrByWorkplaceDate.Of(t));
                if (t.ResultOfTest)
                    ok &= _posByDistrictDate.Delete(PcrPosByDistrictDate.Of(t));
                if (!ok)
                    throw new InvalidOperationException(
                        $"Nepodarilo sa odstrániť všetky indexy pre PCR '{t.UniqueNumberPCR}'.");
            }

            // 4) Zmaž osobu z person indexov
            bool personOk = true;
            personOk &= _idxPersonById.Delete(PersonByUniqueNumber.Of(person));
            personOk &= _idxPersonByBirth.Delete(PersonByBirth.Of(person));

            if (!personOk)
                throw new InvalidOperationException($"Nepodarilo sa odstrániť osobu '{personId}' zo všetkých indexov.");
        }
        public IReadOnlyList<PCRTest> ListAllByWorkplacePeriod(DateTime from, DateTime to, int workCode)
        {
            if (workCode <= 0) throw new ArgumentOutOfRangeException(nameof(workCode));
            if (to < from) (from, to) = (to, from);

            var lo = PcrByWorkplaceDate.Low(workCode, from);
            var hi = PcrByWorkplaceDate.High(workCode, to);

            var results = new List<PCRTest>();

            // prejdeme interval v indexe (pracovisko + dátum)
            foreach (var wrap in _idxByWorkplaceDate.Range(lo, hi))
            {
                var t = wrap.Value; // PCRTest
                                    // istota na pracovisko (ak by interval obsahoval susedné kódy)
                if (t.UniqueNumberPCRPlace == workCode)
                    results.Add(t);
            }

            // deterministické poradie podľa času vykonania
            results.Sort((a, b) => a.DateStartTest.CompareTo(b.DateStartTest));
            return results;
        }
        public IReadOnlyList<(int Region, int SickCount)> ListRegionsBySickCountAtDate(DateTime at, int xDays)
        {
            if (xDays <= 0) throw new ArgumentOutOfRangeException(nameof(xDays));

            // Chorý X dní od pozitívneho testu => okno [at-(X-1), at]
            var from = at.AddDays(-(xDays - 1));

            var lo = PcrByDate.Low(from);
            var hi = PcrByDate.High(at);

            // pre každý kraj držíme množinu unikátnych osôb (podľa UniqueNumberPerson)
            var perRegionPersons = new Dictionary<int, HashSet<string>>();

            foreach (var wrap in _idxByDate.Range(lo, hi))
            {
                var t = wrap.Value;               // PCRTest
                if (!t.ResultOfTest) continue;    // len pozitívne v okne

                int region = t.NumberOfRegion;
                if (!perRegionPersons.TryGetValue(region, out var set))
                {
                    set = new HashSet<string>(StringComparer.Ordinal);
                    perRegionPersons[region] = set;
                }
                set.Add(t.UniqueNumberPerson);    // unikátna osoba v rámci regiónu
            }

            var rows = new List<(int Region, int SickCount)>(perRegionPersons.Count);
            foreach (var kv in perRegionPersons)
                rows.Add((kv.Key, kv.Value.Count));//

            // zoradenie: najviac chorých najskôr, pri zhode podľa kódu kraja vzostupne
            rows.Sort((a, b) =>
            {
                int c = b.SickCount.CompareTo(a.SickCount);
                if (c != 0) return c;
                return a.Region.CompareTo(b.Region);
            });

            return rows;
        }
        public IReadOnlyList<(int District, int SickCount)> ListDistrictsBySickCountAtDate(DateTime at, int xDays)
        {
            if (xDays <= 0) throw new ArgumentOutOfRangeException(nameof(xDays));

            // Chorý X dní od pozitívneho testu => [at-(X-1), at]
            var from = at.AddDays(-(xDays - 1));

            var lo = PcrByDate.Low(from);
            var hi = PcrByDate.High(at);

            // okres -> množina unikátnych osôb
            var perDistrictPersons = new Dictionary<int, HashSet<string>>();

            foreach (var wrap in _idxByDate.Range(lo, hi))
            {
                var t = wrap.Value;            // PCRTest
                if (!t.ResultOfTest) continue; // len pozitívne

                int district = t.NumberOfDistrict;
                if (!perDistrictPersons.TryGetValue(district, out var set))
                {
                    set = new HashSet<string>(StringComparer.Ordinal);
                    perDistrictPersons[district] = set;
                }
                set.Add(t.UniqueNumberPerson); // unikátna osoba v rámci okresu
            }

            var rows = new List<(int District, int SickCount)>(perDistrictPersons.Count);
            foreach (var kv in perDistrictPersons)
                rows.Add((kv.Key, kv.Value.Count));

            // zoradenie: najviac chorých najskôr; pri zhode podľa kódu okresu vzostupne
            rows.Sort((a, b) =>
            {
                int c = b.SickCount.CompareTo(a.SickCount);
                if (c != 0) return c;
                return a.District.CompareTo(b.District);
            });

            return rows;
        }
        public IReadOnlyList<Person> ListSickByRegionAtDate(DateTime at, int region, int xDays)
        {
            if (region <= 0) throw new ArgumentOutOfRangeException(nameof(region));
            if (xDays <= 0) throw new ArgumentOutOfRangeException(nameof(xDays));

            // Chorý X dní od pozitívneho testu => okno [at-(X-1), at]
            var from = at.AddDays(-(xDays - 1));

            var lo = PcrByRegionDate.Low(region, from);
            var hi = PcrByRegionDate.High(region, at);

            var seen = new HashSet<string>(StringComparer.Ordinal); // UniqueNumberPerson
            var persons = new List<Person>();

            foreach (var wrap in _idxByRegionDate.Range(lo, hi))
            {
                var t = wrap.Value; // PCRTest
                if (!t.ResultOfTest) continue;
                if (t.NumberOfRegion != region) continue; // istota na región

                var personId = t.UniqueNumberPerson;
                if (!seen.Add(personId)) continue; // už zarátaný

                if (_idxPersonById.Find(PersonByUniqueNumber.FromKey(personId), out var pWrap) && pWrap.Value != null)
                {
                    persons.Add(pWrap.Value);
                }
                // ak by osoba chýbala, preskočíme
            }

            // Stabilné a čitateľné poradie
            persons.Sort((a, b) =>
            {
                int c = string.Compare(a.LastName, b.LastName, StringComparison.Ordinal);
                if (c != 0) return c;
                c = string.Compare(a.FirstName, b.FirstName, StringComparison.Ordinal);
                if (c != 0) return c;
                return string.Compare(a.UniqueNumber, b.UniqueNumber, StringComparison.Ordinal);
            });

            return persons;
        }
        public IReadOnlyList<Person> ListSickAllAtDate(DateTime at, int xDays)
        {
            if (xDays <= 0) throw new ArgumentOutOfRangeException(nameof(xDays));

            // Chorý X dní od pozitívneho testu => okno [at-(X-1), at]
            var from = at.AddDays(-(xDays - 1));

            var lo = PcrByDate.Low(from);
            var hi = PcrByDate.High(at);

            var seen = new HashSet<string>(StringComparer.Ordinal); // UniqueNumberPerson
            var persons = new List<Person>();

            // prejdeme všetky pozitívne testy v okne
            foreach (var wrap in _idxByDate.Range(lo, hi))
            {
                var t = wrap.Value;            // PCRTest
                if (!t.ResultOfTest) continue; // len pozitívne

                var personId = t.UniqueNumberPerson;
                if (!seen.Add(personId)) continue; // už pridaný

                // dotiahni osobu z indexu osôb
                if (_idxPersonById.Find(PersonByUniqueNumber.FromKey(personId), out var pWrap) && pWrap.Value != null)
                    persons.Add(pWrap.Value);
            }

            // čitateľné deterministické poradie
            persons.Sort((a, b) =>
            {
                int c = string.Compare(a.LastName, b.LastName, StringComparison.Ordinal);
                if (c != 0) return c;
                c = string.Compare(a.FirstName, b.FirstName, StringComparison.Ordinal);
                if (c != 0) return c;
                return string.Compare(a.UniqueNumber, b.UniqueNumber, StringComparison.Ordinal);
            });

            return persons;
        }

        public IReadOnlyList<(Person Person, PCRTest Test)> ListSickAllAtDateWithTest(DateTime at, int xDays)
        {
            if (xDays <= 0) throw new ArgumentOutOfRangeException(nameof(xDays));

            // chorý X dní => okno [at-(X-1), at]
            var from = at.AddDays(-(xDays - 1));

            var lo = PcrByDate.Low(from);
            var hi = PcrByDate.High(at);

            // pre každý okres si držíme test s najvyššou hodnotou
            var bestByDistrict = new Dictionary<int, PCRTest>();

            foreach (var wrap in _idxByDate.Range(lo, hi))
            {
                var t = wrap.Value;                 // PCRTest
                if (!t.ResultOfTest) continue;      // len pozitívne

                int district = t.NumberOfDistrict;
                if (!bestByDistrict.TryGetValue(district, out var curr) || t.ValueOfTest > curr.ValueOfTest)
                    bestByDistrict[district] = t;
            }

            var pairs = new List<(Person Person, PCRTest Test)>(bestByDistrict.Count);

            // dotiahni osoby a postav výsledné dvojice
            foreach (var kv in bestByDistrict.OrderBy(kv => kv.Key)) // zoradenie podľa okresu
            {
                var test = kv.Value;
                var personId = test.UniqueNumberPerson;

                if (_idxPersonById.Find(PersonByUniqueNumber.FromKey(personId), out var pWrap) && pWrap.Value != null)
                {
                    pairs.Add((pWrap.Value, test));
                }
                // ak by osoba chýbala (nemalo by sa stať), okres vynecháme
            }

            return pairs;
        }

        public string SaveAllToFileInAppRoot()
        {
            // 1) cieľový priečinok v koreňovom adresári aplikácie
            var root = AppDomain.CurrentDomain.BaseDirectory;
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var outDir = Path.Combine(root, $"export_{stamp}");
            Directory.CreateDirectory(outDir);

            // 2) priprava writerov
            var personsPath = Path.Combine(outDir, "persons.csv");
            var testsPath = Path.Combine(outDir, "pcr_tests.csv");

            // 3) export OSÔB (z _idxPersonById, celý rozsah)
            using (var sw = new StreamWriter(personsPath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                // hlavička
                sw.WriteLine("UniqueNumber;FirstName;LastName;Birth;Weight");

                var lo = PersonByUniqueNumber.FromKey(string.Empty);
                var hi = PersonByUniqueNumber.FromKey("\uFFFF"); // vysoký sentinel pre všetky ID

                foreach (var wrap in _idxPersonById.Range(lo, hi))
                {
                    var p = wrap.Value;
                    if (p is null) continue;

                    // dátumy v ISO 8601, invariantná kultúra
                    var birthIso = p.Birth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                    sw.WriteLine(string.Join(";",
                        Csv(p.UniqueNumber),
                        Csv(p.FirstName),
                        Csv(p.LastName),
                        Csv(birthIso),
                        Csv(p.Weight.ToString(CultureInfo.InvariantCulture))
                    ));
                }
            }

            // 4) export PCR TESTOV (z _idxByDate, celý rozsah)
            using (var sw = new StreamWriter(testsPath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                // hlavička
                sw.WriteLine("UniqueNumberPCR;UniqueNumberPCRPlace;UniqueNumberPerson;DateStartTest;NumberOfDistrict;NumberOfRegion;ResultOfTest;ValueOfTest;Note");

                var lo = PcrByDate.Low(DateTime.MinValue);
                var hi = PcrByDate.High(DateTime.MaxValue);

                foreach (var wrap in _idxByDate.Range(lo, hi))
                {
                    var t = wrap.Value;
                    if (t is null) continue;

                    var dtIso = t.DateStartTest.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

                    sw.WriteLine(string.Join(";",
                        Csv(t.UniqueNumberPCR.ToString(CultureInfo.InvariantCulture)),
                        Csv(t.UniqueNumberPCRPlace.ToString(CultureInfo.InvariantCulture)), // FIX: bez ?. a bez ??
                        Csv(t.UniqueNumberPerson),
                        Csv(dtIso),
                        Csv(t.NumberOfDistrict.ToString(CultureInfo.InvariantCulture)),
                        Csv(t.NumberOfRegion.ToString(CultureInfo.InvariantCulture)),
                        Csv(t.ResultOfTest ? "1" : "0"),
                        Csv(t.ValueOfTest.ToString(CultureInfo.InvariantCulture)),
                        Csv(t.Note ?? string.Empty)
                    ));
                }
            }

            // 5) vráť cestu k priečinku s exportom (View ho zobrazí)
            return outDir;

            // lokálna pomocná funkcia – jednoduchý CSV escape pre oddeľovač ';' a úvodzovky
            static string Csv(string s)
            {
                if (s is null) return string.Empty;
                bool needsQuotes = s.Contains(';') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
                if (!needsQuotes) return s;
                var esc = s.Replace("\"", "\"\"");
                return $"\"{esc}\"";
            }
        }
        public (int persons, int tests) LoadAllFromFolder(string folderPath, bool clearExisting = true)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) throw new ArgumentNullException(nameof(folderPath));
            if (!Directory.Exists(folderPath)) throw new DirectoryNotFoundException($"Priečinok neexistuje: {folderPath}");

            var personsPath = Path.Combine(folderPath, "persons.csv");
            var testsPath = Path.Combine(folderPath, "pcr_tests.csv");

            if (!File.Exists(personsPath)) throw new FileNotFoundException($"Chýba súbor persons.csv v {folderPath}");
            if (!File.Exists(testsPath)) throw new FileNotFoundException($"Chýba súbor pcr_tests.csv v {folderPath}");

            // 0) Vyčistenie dát, ak chceme načítať "načisto"
            if (clearExisting)
            {
                // Ak tvoje AVL majú Clear(), použi ho. Inak si nechaj Delete cez Range (náročnejšie).
                _idxPersonById.Clear();
                _idxPersonByBirth.Clear();

                _idxByCode.Clear();
                _idxByPerson.Clear();
                _idxByDate.Clear();
                _idxByRegionDate.Clear();
                _idxByDistrictDate.Clear();
                _idxByWorkplaceDate.Clear();

                 _posByDate.Clear();
                _posByRegionDate.Clear();
                _posByDistrictDate.Clear();
            }

            int personsImported = 0;
            int testsImported = 0;

            // 1) PERSONS
            using (var sr = new StreamReader(personsPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                string? line;
                bool headerSkipped = false;

                while ((line = sr.ReadLine()) != null)
                {
                    if (!headerSkipped) { headerSkipped = true; continue; } // preskoč hlavičku

                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = ParseCsvLine(line);
                    if (cols.Count < 5) continue;

                    // persons.csv: UniqueNumber;FirstName;LastName;Birth;Weight
                    var uniqueNumber = cols[0];
                    var firstName = cols[1];
                    var lastName = cols[2];
                    var birth = DateTime.ParseExact(cols[3], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    var weight = double.Parse(cols[4], CultureInfo.InvariantCulture);

                    // vytvor Person so zachovaným UniqueNumber (nepoužívame generovanie ID!)
                    var p = new Person(firstName, lastName, weight, birth, uniqueNumber);

                    // zanes do indexov
                    _idxPersonById.Add(PersonByUniqueNumber.Of(p));
                    _idxPersonByBirth.Add(PersonByBirth.Of(p));

                    personsImported++;
                }
            }

            // 2) PCR TESTS
            using (var sr = new StreamReader(testsPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                string? line;
                bool headerSkipped = false;

                while ((line = sr.ReadLine()) != null)
                {
                    if (!headerSkipped) { headerSkipped = true; continue; } // preskoč hlavičku
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var cols = ParseCsvLine(line);
                    if (cols.Count < 9) continue;

                    // pcr_tests.csv:
                    // UniqueNumberPCR;UniqueNumberPCRPlace;UniqueNumberPerson;DateStartTest;NumberOfDistrict;NumberOfRegion;ResultOfTest;ValueOfTest;Note
                    int uniqueNumberPcr = int.Parse(cols[0], CultureInfo.InvariantCulture);
                    int uniqueNumberPcrPlace = int.Parse(cols[1], CultureInfo.InvariantCulture);
                    string uniqueNumberPerson = cols[2];
                    DateTime dateStartTest = DateTime.ParseExact(cols[3], "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                    int numberOfDistrict = int.Parse(cols[4], CultureInfo.InvariantCulture);
                    int numberOfRegion = int.Parse(cols[5], CultureInfo.InvariantCulture);
                    bool resultOfTest = cols[6] == "1" || cols[6].Equals("true", StringComparison.OrdinalIgnoreCase);
                    double valueOfTest = double.Parse(cols[7], CultureInfo.InvariantCulture);
                    string note = cols[8];

                    var pcr = new PCRTest(
                        dateStartTest: dateStartTest,
                        numberOfDistrict: numberOfDistrict,
                        numberOfRegion: numberOfRegion,
                        resultOfTest: resultOfTest,
                        valueOfTest: valueOfTest,
                        note: note,
                        uniqueNumberPerson: uniqueNumberPerson,
                        uniqueNumberPcr: uniqueNumberPcr,
                        uniqueNumberPcrPlace: uniqueNumberPcrPlace
                    );

                    // indexácia do všetkých PCR indexov
                    _idxByCode.Add(PcrByCode.Of(pcr));
                    _idxByPerson.Add(PcrByPersonDate.Of(pcr));
                    _idxByDate.Add(PcrByDate.Of(pcr));
                    _idxByRegionDate.Add(PcrByRegionDate.Of(pcr));
                    _idxByDistrictDate.Add(PcrByDistrictDate.Of(pcr));
                    _idxByWorkplaceDate.Add(PcrByWorkplaceDate.Of(pcr));
                    
                    // napoj test do osobného stromu osoby (PO indexácii; POUŽI INÉ MENO PREMENNEJ)
                    if (_idxPersonById.Find(PersonByUniqueNumber.FromKey(uniqueNumberPerson), out var pWrap)
                        && pWrap.Value is Person person)
                    {
                        person.personPcrTests.Add(PcrByCode.Of(pcr));
                        if (pcr.ResultOfTest)
                            UpdateSickIndexForPositive(person, pcr);
                    }

                    // pozitívny index
                    if (pcr.ResultOfTest) 
                    {
                        _posByDistrictDate.Add(PcrPosByDistrictDate.Of(pcr));
                        _posByRegionDate.Add(PcrPosByRegionDate.Of(pcr));
                        _posByDate.Add(PcrPosByDate.Of(pcr));
                        
                    }
                        

                    testsImported++;
                }
            }

            return (personsImported, testsImported);
        }

        // Jednoduchý CSV parser pre ';' s podporou úvodzoviek a zdvojených úvodzoviek
        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(line)) { result.Add(string.Empty); return result; }

            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // zdvojená úvodzovka -> literal "
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == ';')
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            result.Add(sb.ToString());
            return result;
        }

        public PCRTest AssignPcrToPerson(string personId, int pcrCode)
        {
            if (string.IsNullOrWhiteSpace(personId))
                throw new ArgumentNullException(nameof(personId));
            if (pcrCode <= 0)
                throw new ArgumentOutOfRangeException(nameof(pcrCode), "Kód PCR testu musí byť kladné celé číslo.");

            // 1) nová osoba
            if (!_idxPersonById.Find(PersonByUniqueNumber.FromKey(personId), out var personWrap) || personWrap.Value is null)
                throw new InvalidOperationException($"Osoba s ID '{personId}' neexistuje.");
            var newOwner = personWrap.Value;

            // 2) pôvodný test
            if (!_idxByCode.Find(PcrByCode.FromKey(pcrCode), out var testWrap) || testWrap.Value is null)
                throw new InvalidOperationException($"PCR test s kódom {pcrCode} neexistuje.");

            var old = testWrap.Value;

            // rýchly no-op: ak už patrí tej istej osobe, nič nemeníme
            if (string.Equals(old.UniqueNumberPerson, newOwner.UniqueNumber, StringComparison.Ordinal))
                return old;

            var previousOwnerId = old.UniqueNumberPerson;

            // 3) vytvor aktualizovaný test s novým vlastníkom
            var updated = new PCRTest(
                dateStartTest: old.DateStartTest,
                numberOfDistrict: old.NumberOfDistrict,
                numberOfRegion: old.NumberOfRegion,
                resultOfTest: old.ResultOfTest,
                valueOfTest: old.ValueOfTest,
                note: old.Note,
                uniqueNumberPerson: newOwner.UniqueNumber,
                uniqueNumberPcr: old.UniqueNumberPCR,
                uniqueNumberPcrPlace: old.UniqueNumberPCRPlace
            );

            // 4) aktualizácia indexov
            _idxByCode.Delete(PcrByCode.FromKey(old.UniqueNumberPCR));
            _idxByCode.Add(PcrByCode.Of(updated));

            _idxByPerson.Delete(PcrByPersonDate.Of(old));
            _idxByPerson.Add(PcrByPersonDate.Of(updated));

            _idxByDate.Delete(PcrByDate.Of(old));
            _idxByDate.Add(PcrByDate.Of(updated));

            _idxByRegionDate.Delete(PcrByRegionDate.Of(old));
            _idxByRegionDate.Add(PcrByRegionDate.Of(updated));

            _idxByDistrictDate.Delete(PcrByDistrictDate.Of(old));
            _idxByDistrictDate.Add(PcrByDistrictDate.Of(updated));

            _idxByWorkplaceDate.Delete(PcrByWorkplaceDate.Of(old));
            _idxByWorkplaceDate.Add(PcrByWorkplaceDate.Of(updated));

            // 5) uprav osobné stromy
            if (!string.IsNullOrEmpty(previousOwnerId) &&
                _idxPersonById.Find(PersonByUniqueNumber.FromKey(previousOwnerId), out var prevWrap) &&
                prevWrap.Value is Person prevOwner)
            {
                prevOwner.personPcrTests.Delete(PcrByCode.FromKey(pcrCode));
            }
            // pridaj test novej osobe
            newOwner.personPcrTests.Add(PcrByCode.Of(updated));

            return updated;
        }
        private void UpdateSickIndexForPositive(Person person, PCRTest t)
        {
            if (!t.ResultOfTest) return;                     // len pozitívne
            if (string.IsNullOrEmpty(person.UniqueNumber))   
                return;

            int district = t.NumberOfDistrict;
            string pid = person.UniqueNumber;

            // nájdi existujúci záznam pre (district, person) – maximálne 1
            var loDist = SickByDistrictDate.Low(district, DateTime.MinValue);
            var hiDist = SickByDistrictDate.High(district, DateTime.MaxValue);

            SickByDistrictDate? existing = null;
            foreach (var w in _idxSickByDistrictDate.Range(loDist, hiDist))
            {
                if (w.UniqueNumberPerson == pid)
                {
                    existing = w;
                    break;
                }
            }

            var newWrap = SickByDistrictDate.Of(person, district, t.DateStartTest);

            if (existing == null)
            {
                // osoba ešte nemá pozitívny test v tomto okrese → pridáme
                _idxSickByDistrictDate.Add(newWrap);
            }
            else if (newWrap.LastPositiveDate > existing.LastPositiveDate)
            {
                // nový test je novší → aktualizujeme dátum
                _idxSickByDistrictDate.Delete(existing);
                _idxSickByDistrictDate.Add(newWrap);
            }
        }
    }
}
