using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class PcrByWorkplaceDate : IMyComparable<PcrByWorkplaceDate>
    {
        public PCRTest? Value { get; }            // null pri sentineli
        public int UniqueNumberPCRPlace { get; }
        public DateTime DateStartTest { get; }
        public int UniqueNumberPCR { get; }

        private PcrByWorkplaceDate(PCRTest? v, int place, DateTime dt, int code)
        {
            Value = v;
            UniqueNumberPCRPlace = place;
            DateStartTest = dt;
            UniqueNumberPCR = code;
        }

        // bežný záznam
        public static PcrByWorkplaceDate Of(PCRTest t) =>
            new PcrByWorkplaceDate(t, t.UniqueNumberPCRPlace, t.DateStartTest, t.UniqueNumberPCR);

        // kľúče / sentinely
        public static PcrByWorkplaceDate KeyPlace(int place) =>
            new PcrByWorkplaceDate(null, place, DateTime.MinValue, int.MinValue);

        public static PcrByWorkplaceDate KeyExact(int place, DateTime dt, int pcrCode) =>
            new PcrByWorkplaceDate(null, place, dt, pcrCode);

        public static PcrByWorkplaceDate Low(int place, DateTime from) =>
            new PcrByWorkplaceDate(null, place, from, int.MinValue);

        public static PcrByWorkplaceDate High(int place, DateTime toInclusive) =>
            new PcrByWorkplaceDate(null, place, toInclusive, int.MaxValue);

        public int CompareTo(PcrByWorkplaceDate other)
        {
            int c = UniqueNumberPCRPlace.CompareTo(other.UniqueNumberPCRPlace);
            if (c != 0) return c;
            c = DateStartTest.CompareTo(other.DateStartTest);
            if (c != 0) return c;
            return UniqueNumberPCR.CompareTo(other.UniqueNumberPCR);
        }

        public override bool Equals(object obj) => obj is PcrByWorkplaceDate o && CompareTo(o) == 0;

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = 31 * h + UniqueNumberPCRPlace.GetHashCode();
                h = 31 * h + DateStartTest.GetHashCode();
                h = 31 * h + UniqueNumberPCR.GetHashCode();
                return h;
            }
        }

        public override string ToString() =>
            $"PcrByWorkplaceDate(W{UniqueNumberPCRPlace}, {DateStartTest:yyyy-MM-dd HH:mm}, {UniqueNumberPCR})";
    }
}
