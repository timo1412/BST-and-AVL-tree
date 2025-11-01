using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class PcrByCode : IMyComparable<PcrByCode>
    {
        public PCRTest? Value { get; } 
        public int UniqueNumberPCR { get; }
        public DateTime DateStartTest { get; }
        public string UniqueNumberPerson { get; }

        // súkromný konštruktor (použitý aj pre sentinel)
        private PcrByCode(PCRTest? value, int code, DateTime dt, string personId)
        {
            Value = value;
            UniqueNumberPCR = code;
            DateStartTest = dt;
            UniqueNumberPerson = personId ?? "";
        }

        // bežný záznam (zabalí existujúci PCRTest)
        public static PcrByCode Of(PCRTest t) =>
            new PcrByCode(t, t.UniqueNumberPCR, t.DateStartTest, t.UniqueNumberPerson);

        // sentinel — len podľa kľúča (napr. Find/Delete)
        public static PcrByCode Key(int code) =>
            new PcrByCode(null, code, DateTime.MinValue, "");

        public int CompareTo(PcrByCode other)
        {
            int c = UniqueNumberPCR.CompareTo(other.UniqueNumberPCR);
            if (c != 0) return c;
            // tie-breakery pre totálne poradie (ak by sa v budúcnosti kód nereálne duplicitne objavil)
            c = DateStartTest.CompareTo(other.DateStartTest);
            if (c != 0) return c;
            return string.Compare(UniqueNumberPerson, other.UniqueNumberPerson, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => obj is PcrByCode o && CompareTo(o) == 0;

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = 31 * h + UniqueNumberPCR.GetHashCode();
                h = 31 * h + DateStartTest.GetHashCode();
                h = 31 * h + (UniqueNumberPerson?.GetHashCode() ?? 0);
                return h;
            }
        }

        public override string ToString() => $"PcrByCode({UniqueNumberPCR})";
    }
}
