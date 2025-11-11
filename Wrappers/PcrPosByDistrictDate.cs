using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class PcrPosByDistrictDate : IMyComparable<PcrPosByDistrictDate>
    {
        public PCRTest? Value { get; }            // null pri sentineli
        public int NumberOfDistrict { get; }
        public DateTime DateStartTest { get; }
        public int UniqueNumberPCR { get; }

        private PcrPosByDistrictDate(PCRTest? v, int district, DateTime dt, int code)
        {
            Value = v;
            NumberOfDistrict = district;
            DateStartTest = dt;
            UniqueNumberPCR = code;
        }

        // bežný záznam (volaj len pre pozitívne testy)
        public static PcrPosByDistrictDate Of(PCRTest t) =>
            new PcrPosByDistrictDate(t, t.NumberOfDistrict, t.DateStartTest, t.UniqueNumberPCR);

        // kľúč len podľa okresu (na hrubé hľadanie)
        public static PcrPosByDistrictDate KeyDistrict(int district) =>
            new PcrPosByDistrictDate(null, district, DateTime.MinValue, int.MinValue);

        // presný kľúč (poznáš okres, dátum a kód testu)
        public static PcrPosByDistrictDate KeyExact(int district, DateTime dt, int pcrCode) =>
            new PcrPosByDistrictDate(null, district, dt, pcrCode);

        // intervalové hranice v rámci jedného okresu
        public static PcrPosByDistrictDate Low(int district, DateTime from) =>
            new PcrPosByDistrictDate(null, district, from, int.MinValue);

        public static PcrPosByDistrictDate High(int district, DateTime toInclusive) =>
            new PcrPosByDistrictDate(null, district, toInclusive, int.MaxValue);

        public int CompareTo(PcrPosByDistrictDate other)
        {
            int c = NumberOfDistrict.CompareTo(other.NumberOfDistrict);
            if (c != 0) return c;
            c = DateStartTest.CompareTo(other.DateStartTest);
            if (c != 0) return c;
            return UniqueNumberPCR.CompareTo(other.UniqueNumberPCR);
        }

        public override bool Equals(object obj) => obj is PcrPosByDistrictDate o && CompareTo(o) == 0;

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
            $"PcrPosByDistrictDate(D{NumberOfDistrict}, {DateStartTest:yyyy-MM-dd HH:mm}, {UniqueNumberPCR})";
    }
}
