using SemestralnaPracaAUS2.Model;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SemestralnaPracaAUS2.TestData.SeedContracts;

namespace SemestralnaPracaAUS2.Architecture
{
    public sealed class ApplicationView
    {
        private readonly MyDatabaseModel _model;

        public ApplicationView(MyDatabaseModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
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

        public (bool ok, string? error, IReadOnlyList<PCRTest>? tests)
            ListPcrForPerson(string personId)
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

        public readonly record struct PcrInputDto(
            DateTime DateStartTest,
            int NumberOfDistrict,
            int NumberOfRegion,
            bool ResultOfTest,
            double ValueOfTest,
            string Note
        );
    }
}
