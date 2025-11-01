using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class PcrByPersonDate : IMyComparable<PcrByPersonDate>
    {
        public PCRTest? Value { get; }           // null pri sentineli
        public string UniqueNumberPerson { get; }
        public DateTime DateStartTest { get; }
        public int UniqueNumberPCR { get; }

        private PcrByPersonDate(PCRTest? v, string personId, DateTime dt, int pcrCode)
        {
            Value = v;
            UniqueNumberPerson = personId ?? "";
            DateStartTest = dt;
            UniqueNumberPCR = pcrCode;
        }

        // bežný záznam
        public static PcrByPersonDate Of(PCRTest t) =>
            new PcrByPersonDate(t, t.UniqueNumberPerson, t.DateStartTest, t.UniqueNumberPCR);

        // kľúč – len podľa osoby (na presné Find/Delete, ak vieš aj presný kód, použi KeyRange)
        public static PcrByPersonDate Key(string personId) =>
            new PcrByPersonDate(null, personId, DateTime.MinValue, int.MinValue);

        // dolná/hor. hranica pre interval podľa osoby a dátumu
        public static PcrByPersonDate Low(string personId, DateTime from) =>
            new PcrByPersonDate(null, personId, from, int.MinValue);

        public static PcrByPersonDate High(string personId, DateTime toInclusive) =>
            new PcrByPersonDate(null, personId, toInclusive, int.MaxValue);

        public int CompareTo(PcrByPersonDate other)
        {
            int c = string.Compare(UniqueNumberPerson, other.UniqueNumberPerson, StringComparison.Ordinal);
            if (c != 0) return c;
            c = DateStartTest.CompareTo(other.DateStartTest);
            if (c != 0) return c;
            return UniqueNumberPCR.CompareTo(other.UniqueNumberPCR);
        }

        public override bool Equals(object obj) => obj is PcrByPersonDate o && CompareTo(o) == 0;

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = 31 * h + (UniqueNumberPerson?.GetHashCode() ?? 0);
                h = 31 * h + DateStartTest.GetHashCode();
                h = 31 * h + UniqueNumberPCR.GetHashCode();
                return h;
            }
        }

        public override string ToString() =>
            $"PcrByPersonDate({UniqueNumberPerson}, {DateStartTest:yyyy-MM-dd HH:mm}, {UniqueNumberPCR})";
    }
}
