using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class PersonByBirth : IMyComparable<PersonByBirth>
    {
        public Person? Value { get; }
        public DateTime Birth { get; }
        public string UniqueNumber { get; }
        public string LastName { get; }
        public string FirstName { get; }

        private PersonByBirth(Person? v, DateTime birth, string id, string last, string first)
        {
            Value = v;
            Birth = birth;
            UniqueNumber = id ?? "";
            LastName = last ?? "";
            FirstName = first ?? "";
        }

        public static PersonByBirth Of(Person p) =>
            new PersonByBirth(p, p.Birth, p.UniqueNumber ?? "", p.LastName, p.FirstName);

        // kľúčový sentinel pre intervalové dotazy podľa dátumu narodenia
        public static PersonByBirth Key(DateTime birth, string id = "") =>
            new PersonByBirth(null, birth, id ?? "", "", "");

        public int CompareTo(PersonByBirth other)
        {
            int c = Birth.CompareTo(other.Birth);
            if (c != 0) return c;
            c = string.Compare(UniqueNumber, other.UniqueNumber, StringComparison.Ordinal);
            if (c != 0) return c;
            c = string.Compare(LastName, other.LastName, StringComparison.Ordinal);
            if (c != 0) return c;
            return string.Compare(FirstName, other.FirstName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => obj is PersonByBirth o && CompareTo(o) == 0;
        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = 31 * h + Birth.GetHashCode();
                h = 31 * h + UniqueNumber.GetHashCode();
                h = 31 * h + LastName.GetHashCode();
                h = 31 * h + FirstName.GetHashCode();
                return h;
            }
        }
    }
}
