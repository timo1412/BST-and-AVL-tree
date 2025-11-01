using SemestralnaPracaAUS2.Model;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
