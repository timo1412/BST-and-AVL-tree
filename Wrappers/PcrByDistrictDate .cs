using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class PcrByDistrictDate : IMyComparable<PcrByDistrictDate>
    {
        public PCRTest? Value { get; }            // null pri sentineli
        public int NumberOfDistrict { get; }
        public DateTime DateStartTest { get; }
        public int UniqueNumberPCR { get; }

        private PcrByDistrictDate(PCRTest? v, int district, DateTime dt, int code)
        {
            Value = v;
            NumberOfDistrict = district;
            DateStartTest = dt;
            UniqueNumberPCR = code;
        }

        // bežný záznam
        public static PcrByDistrictDate Of(PCRTest t) =>
            new PcrByDistrictDate(t, t.NumberOfDistrict, t.DateStartTest, t.UniqueNumberPCR);

        // kľúč len podľa okresu (na hrubé hľadanie; na presné použij KeyExact)
        public static PcrByDistrictDate KeyDistrict(int district) =>
            new PcrByDistrictDate(null, district, DateTime.MinValue, int.MinValue);

        // presný kľúč (poznáš okres, dátum a kód)
        public static PcrByDistrictDate KeyExact(int district, DateTime dt, int pcrCode) =>
            new PcrByDistrictDate(null, district, dt, pcrCode);

        // intervaly v rámci okresu
        public static PcrByDistrictDate Low(int district, DateTime from) =>
            new PcrByDistrictDate(null, district, from, int.MinValue);

        public static PcrByDistrictDate High(int district, DateTime toInclusive) =>
            new PcrByDistrictDate(null, district, toInclusive, int.MaxValue);

        public int CompareTo(PcrByDistrictDate other)
        {
            int c = NumberOfDistrict.CompareTo(other.NumberOfDistrict);
            if (c != 0) return c;
            c = DateStartTest.CompareTo(other.DateStartTest);
            if (c != 0) return c;
            return UniqueNumberPCR.CompareTo(other.UniqueNumberPCR);
        }

        public override bool Equals(object obj) => obj is PcrByDistrictDate o && CompareTo(o) == 0;

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = 31 * h + NumberOfDistrict.GetHashCode();
                h = 31 * h + DateStartTest.GetHashCode();
                h = 31 * h + UniqueNumberPCR.GetHashCode();
                return h;
            }
        }

        public override string ToString() =>
            $"PcrByDistrictDate(D{NumberOfDistrict}, {DateStartTest:yyyy-MM-dd HH:mm}, {UniqueNumberPCR})";
    }
}
