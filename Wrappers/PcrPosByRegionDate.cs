using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class PcrPosByRegionDate : IMyComparable<PcrPosByRegionDate>
    {
        public PCRTest? Value { get; }            // null pri sentineli
        public int NumberOfRegion { get; }
        public DateTime DateStartTest { get; }
        public int UniqueNumberPCR { get; }

        private PcrPosByRegionDate(PCRTest? v, int region, DateTime dt, int code)
        {
            Value = v;
            NumberOfRegion = region;
            DateStartTest = dt;
            UniqueNumberPCR = code;
        }

        // bežný záznam
        public static PcrPosByRegionDate Of(PCRTest t) =>
            new PcrPosByRegionDate(t, t.NumberOfRegion, t.DateStartTest, t.UniqueNumberPCR);

        // kľúč len podľa kraja (na hrubé vyhľadanie)
        public static PcrPosByRegionDate KeyRegion(int region) =>
            new PcrPosByRegionDate(null, region, DateTime.MinValue, int.MinValue);

        // presný kľúč (poznáš kraj, dátum a kód testu)
        public static PcrPosByRegionDate KeyExact(int region, DateTime dt, int pcrCode) =>
            new PcrPosByRegionDate(null, region, dt, pcrCode);

        // intervalové hranice v rámci jedného kraja
        public static PcrPosByRegionDate Low(int region, DateTime from) =>
            new PcrPosByRegionDate(null, region, from, int.MinValue);

        public static PcrPosByRegionDate High(int region, DateTime toInclusive) =>
            new PcrPosByRegionDate(null, region, toInclusive, int.MaxValue);

        public int CompareTo(PcrPosByRegionDate other)
        {
            int c = NumberOfRegion.CompareTo(other.NumberOfRegion);
            if (c != 0) return c;
            c = DateStartTest.CompareTo(other.DateStartTest);
            if (c != 0) return c;
            return UniqueNumberPCR.CompareTo(other.UniqueNumberPCR);
        }

        public override bool Equals(object obj) => obj is PcrPosByRegionDate o && CompareTo(o) == 0;

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = 31 * h + NumberOfRegion.GetHashCode();
                h = 31 * h + DateStartTest.GetHashCode();
                h = 31 * h + UniqueNumberPCR.GetHashCode();
                return h;
            }
        }

        public override string ToString() =>
            $"PcrPosByRegionDate(R{NumberOfRegion}, {DateStartTest:yyyy-MM-dd HH:mm}, {UniqueNumberPCR})";
    }
}
