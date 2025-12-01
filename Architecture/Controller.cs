using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SemestralnaPracaAUS2.TestData.SeedContracts;

namespace SemestralnaPracaAUS2.Architecture
{
    public sealed class Controller
    {
        private readonly MyDatabaseModel _model;

        public Controller()
        {
            _model = new MyDatabaseModel();
        }
        /// <summary>
        /// Vloží PCR test do databázy cez model.
        /// Prijíma iba DTO (kópiu), UI tak nikdy nemanipuluje s internými entitami.
        /// </summary>
        public (bool ok, string? error) InsertPcr(PcrInputDto dto)
        {
            try
            {
                var entity = new PCRTest(
                    dto.DateStartTest,
                    dto.PersonId,
                    dto.NumberOfDistrict,
                    dto.NumberOfRegion,
                    dto.ResultOfTest,
                    dto.ValueOfTest,
                    dto.Note
                );

                _model.InsertPcr(entity);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool ok, string? error, (Person Person, PCRTest Pcr)? result) FindPcrForPerson(string personId, int pcrCode)
        {
            try
            {
                var (person, pcr) = _model.FindPcrByCodeForPerson(personId, pcrCode);
                return (true, null, (person, pcr));
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<PCRTest>? tests) ListPositiveByDistrictPeriod(DateTime from, DateTime to, int district)
        {
            try
            {
                var tests = _model.ListPositiveByDistrictPeriod(from, to, district);
                return (true, null, tests);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public (bool ok, string? error, SeedResult? result) SeedRandom(SeedRequest req)
        {
            try
            {
                var res = _model.SeedRandom(req);     // implementáciu doplníme neskôr
                return (true, null, res);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public (bool ok, string? error, IReadOnlyList<PCRTest>? tests)ListPcrForPerson(string personId)
        {
            try
            {
                var tests = _model.ListPcrForPerson(personId);
                return (true, null, tests);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<PCRTest>? tests) ListAllByDistrictPeriod(DateTime from, DateTime to, int district)
        {
            try
            {
                var tests = _model.ListAllByDistrictPeriod(from, to, district); 
                return (true, null, tests);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<PCRTest>? tests)ListPositiveByRegionPeriod(DateTime from, DateTime to, int region)
        {
            try
            {
                var tests = _model.ListPositiveByRegionPeriod(from, to, region);
                return (true, null, tests);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<PCRTest>? tests)ListPositiveAllPeriod(DateTime from, DateTime to)
        {
            try
            {
                var tests = _model.ListPositiveAllPeriod(from, to);
                return (true, null, tests);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public (bool ok, string? error, IReadOnlyList<PCRTest>? tests)ListAllByRegionPeriod(DateTime from, DateTime to, int region)
        {
            try
            {
                var tests = _model.ListAllByRegionPeriod(from, to, region);
                return (true, null, tests);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<PCRTest>? tests) ListAllPeriod(DateTime from, DateTime to)
        {
            try
            {
                var tests = _model.ListAllPeriod(from, to);
                return (true, null, tests);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<(Person Person, PCRTest Test)>? persons)ListSickByDistrictAtDate(DateTime at,int district, int xDays)
        {
            try
            {
                var persons = _model.ListSickByDistrictAtDate(at, district, xDays);
                return (true, null, persons);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<(Person Person, PCRTest Test)>? pairs)ListSickByDistrictAtDateWithTest(DateTime at, int district, int xDays)
        {
            try
            {
                var pairs = _model.ListSickByDistrictAtDateWithTest(at, district, xDays);
                return (true, null, pairs);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, Person? person)AddPerson(string firstName, string lastName, DateTime birthDate, double weight)
        {
            try
            {
                var person = _model.AddPerson(firstName, lastName, birthDate, weight);
                return (true, null, person);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, PCRTest? test)FindPcrByCode(int code)
        {
            try
            {
                var test = _model.FindPcrByCode(code);
                return (true, null, test);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, PCRTest? deleted)DeletePcrByCode(int code)
        {
            try
            {
                var deleted = _model.DeletePcrByCode(code);
                return (true, null, deleted);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error) DeletePersonWithTests(string personId)
        {
            try
            {
                _model.DeletePersonWithTests(personId);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool ok, string? error, IReadOnlyList<PCRTest>? tests)ListAllByWorkplacePeriod(DateTime from, DateTime to, int workCode)
        {
            try
            {
                var tests = _model.ListAllByWorkplacePeriod(from, to, workCode);
                return (true, null, tests);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<(int Region, int SickCount)>? rows)ListRegionsBySickCountAtDate(DateTime at, int xDays)
        {
            try
            {
                var rows = _model.ListRegionsBySickCountAtDate(at, xDays);
                return (true, null, rows);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<(int District, int SickCount)>? rows)ListDistrictsBySickCountAtDate(DateTime at, int xDays)
        {
            try
            {
                var rows = _model.ListDistrictsBySickCountAtDate(at, xDays);
                return (true, null, rows);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<(Person Person, PCRTest Test)>? rows)ListSickByRegionAtDateWithTest(DateTime at, int region, int xDays)
        {
            try
            {
                var pairs = _model.ListSickByRegionAtDateWithTest(at, region, xDays);
                return (true, null, pairs);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public (bool ok, string? error, IReadOnlyList<(Person Person, PCRTest Test)>? rows)
            ListSickAllAtDate(DateTime at, int xDays)
        {
            try
            {
                var rows = _model.ListSickAllAtDate(at, xDays);
                return (true, null, rows);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, IReadOnlyList<(Person Person, PCRTest Test)>? pairs)ListSickAllAtDateWithTest(DateTime at, int xDays)
        {
            try
            {
                var pairs = _model.ListSickAllAtDateWithTest(at, xDays);
                return (true, null, pairs);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, string? path) SaveAllToFileInAppRoot()
        {
            try
            {
                var path = _model.SaveAllToFileInAppRoot();
                return (true, null, path);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, (int persons, int tests)? result)LoadAllFromFolder(string folderPath, bool clearExisting = true)
        {
            try
            {
                var (persons, tests) = _model.LoadAllFromFolder(folderPath, clearExisting);
                return (true, null, (persons, tests));
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, PCRTest? updatedTest) AssignPcrToPerson(string personId, int pcrCode)
        {
            try
            {
                var updated = _model.AssignPcrToPerson(personId, pcrCode);

                return (true, null, updated);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        public (bool ok, string? error, PCRTest? updated) UpdatePcr(int code,int region,int district,bool result,double value,string note)
        {
            try
            {
                var updated = _model.UpdatePcr(code, region, district, result, value, note);
                return (true, null, updated);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public readonly record struct PcrInputDto(
            DateTime DateStartTest,
            string PersonId,
            int NumberOfDistrict,
            int NumberOfRegion,
            bool ResultOfTest,
            double ValueOfTest,
            string Note
        );
    }
}
