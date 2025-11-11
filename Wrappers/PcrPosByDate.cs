using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class PcrPosByDate : IMyComparable<PcrPosByDate>
    {
        public PCRTest? Value { get; }            // null pri sentineli
        public DateTime DateStartTest { get; }
        public int UniqueNumberPCR { get; }
        public string UniqueNumberPerson { get; }

        private PcrPosByDate(PCRTest? v, DateTime dt, int code, string personId)
        {
            Value = v;
            DateStartTest = dt;
            UniqueNumberPCR = code;
            UniqueNumberPerson = personId ?? "";
        }

        // bežný záznam
        public static PcrPosByDate Of(PCRTest t) =>
            new PcrPosByDate(t, t.DateStartTest, t.UniqueNumberPCR, t.UniqueNumberPerson);

        // presný kľúč (ak vieš aj PCR kód a prípadne ID osoby)
        public static PcrPosByDate Key(DateTime dt, int pcrCode, string personId = "") =>
            new PcrPosByDate(null, dt, pcrCode, personId ?? "");

        // dolná/ horná hranica pre intervaly podľa dátumu
        public static PcrPosByDate Low(DateTime from) =>
            new PcrPosByDate(null, from, int.MinValue, "");

        public static PcrPosByDate High(DateTime toInclusive) =>
            new PcrPosByDate(null, toInclusive, int.MaxValue, "\uFFFF");

        public int CompareTo(PcrPosByDate other)
        {
            int c = DateStartTest.CompareTo(other.DateStartTest);
            if (c != 0) return c;
            c = UniqueNumberPCR.CompareTo(other.UniqueNumberPCR);
            if (c != 0) return c;
            return string.Compare(UniqueNumberPerson, other.UniqueNumberPerson, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => obj is PcrPosByDate o && CompareTo(o) == 0;

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = 31 * h + DateStartTest.GetHashCode();
                h = 31 * h + UniqueNumberPCR.GetHashCode();
                h = 31 * h + (UniqueNumberPerson?.GetHashCode() ?? 0);
                return h;
            }
        }

        public override string ToString() =>
            $"PcrPosByDate({DateStartTest:yyyy-MM-dd HH:mm}, {UniqueNumberPCR}, {UniqueNumberPerson})";
    }
}
