using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class PcrByRegionDate : IMyComparable<PcrByRegionDate>
    {
        public PCRTest? Value { get; }            // null pri sentineli
        public int NumberOfRegion { get; }
        public DateTime DateStartTest { get; }
        public int UniqueNumberPCR { get; }

        private PcrByRegionDate(PCRTest? v, int region, DateTime dt, int code)
        {
            Value = v;
            NumberOfRegion = region;
            DateStartTest = dt;
            UniqueNumberPCR = code;
        }

        // bežný záznam
        public static PcrByRegionDate Of(PCRTest t) =>
            new PcrByRegionDate(t, t.NumberOfRegion, t.DateStartTest, t.UniqueNumberPCR);

        // kľúč len podľa kraja (napr. na vyhľadanie/mazanie s presným kódom použi KeyExact)
        public static PcrByRegionDate KeyRegion(int region) =>
            new PcrByRegionDate(null, region, DateTime.MinValue, int.MinValue);

        // presný kľúč (poznáš kraj, dátum, kód)
        public static PcrByRegionDate KeyExact(int region, DateTime dt, int pcrCode) =>
            new PcrByRegionDate(null, region, dt, pcrCode);

        // dolná/hor. hranica pre intervaly v rámci kraja
        public static PcrByRegionDate Low(int region, DateTime from) =>
            new PcrByRegionDate(null, region, from, int.MinValue);

        public static PcrByRegionDate High(int region, DateTime toInclusive) =>
            new PcrByRegionDate(null, region, toInclusive, int.MaxValue);

        public int CompareTo(PcrByRegionDate other)
        {
            int c = NumberOfRegion.CompareTo(other.NumberOfRegion);
            if (c != 0) return c;
            c = DateStartTest.CompareTo(other.DateStartTest);
            if (c != 0) return c;
            return UniqueNumberPCR.CompareTo(other.UniqueNumberPCR);
        }

        public override bool Equals(object obj) => obj is PcrByRegionDate o && CompareTo(o) == 0;

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
            $"PcrByRegionDate(R{NumberOfRegion}, {DateStartTest:yyyy-MM-dd HH:mm}, {UniqueNumberPCR})";
    }
}
