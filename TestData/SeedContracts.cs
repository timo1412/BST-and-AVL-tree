using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.TestData
{
    public class SeedContracts
    {
        public readonly record struct SeedRequest(
            int Persons,
            int PcrPerPerson,
            DateTime DateFrom,
            DateTime DateTo,
            TimeSpan DayTimeFrom,
            TimeSpan DayTimeTo,
            double PositiveRatio
        );

        public readonly record struct SeedResult(
            int PersonsInserted,
            int PcrInserted
        );
    }
}
