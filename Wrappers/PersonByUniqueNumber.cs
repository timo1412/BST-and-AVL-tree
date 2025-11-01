using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class PersonByUniqueNumber : IMyComparable<PersonByUniqueNumber>
    {
        public Person? Value { get; }                 // reálny záznam (null pri sentinelovi)
        public string UniqueNumber { get; }           // primárny kľúč
        public DateTime Birth { get; }                // sekundárny kľúč (stabilizácia)
        public string LastName { get; }               // doplnkové väzby
        public string FirstName { get; }

        private PersonByUniqueNumber(Person? v, string id, DateTime birth, string last, string first)
        {
            Value = v;
            UniqueNumber = id ?? "";
            Birth = birth;
            LastName = last ?? "";
            FirstName = first ?? "";
        }

        // bežný záznam
        public static PersonByUniqueNumber Of(Person p) =>
            new PersonByUniqueNumber(p, p.UniqueNumber ?? "", p.Birth, p.LastName, p.FirstName);

        // „kľúčový“ sentinel (napr. pre Find/Range)
        public static PersonByUniqueNumber Key(string id, DateTime? birth = null) =>
            new PersonByUniqueNumber(null, id ?? "", birth ?? DateTime.MinValue, "", "");

        public int CompareTo(PersonByUniqueNumber other)
        {
            int c = string.Compare(UniqueNumber, other.UniqueNumber, StringComparison.Ordinal);
            if (c != 0) return c;
            c = Birth.CompareTo(other.Birth);
            if (c != 0) return c;
            c = string.Compare(LastName, other.LastName, StringComparison.Ordinal);
            if (c != 0) return c;
            return string.Compare(FirstName, other.FirstName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => obj is PersonByUniqueNumber o && CompareTo(o) == 0;
        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = 31 * h + UniqueNumber.GetHashCode();
                h = 31 * h + Birth.GetHashCode();
                h = 31 * h + LastName.GetHashCode();
                h = 31 * h + FirstName.GetHashCode();
                return h;
            }
        }
    }
}
