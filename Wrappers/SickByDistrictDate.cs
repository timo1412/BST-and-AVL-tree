using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Wrappers
{
    public sealed class SickByDistrictDate : IMyComparable<SickByDistrictDate>
    {
        public Person? Value { get; }           
        public int NumberOfDistrict { get; }
        public DateTime LastPositiveDate { get; }
        public string UniqueNumberPerson { get; }

        private SickByDistrictDate(Person? v, int district, DateTime dt, string personId)
        {
            Value = v;
            NumberOfDistrict = district;
            LastPositiveDate = dt.Date;
            UniqueNumberPerson = personId ?? "";
        }

        // plnohodnotný záznam
        public static SickByDistrictDate Of(Person p, int district, DateTime lastPositiveDate) =>
            new SickByDistrictDate(p, district, lastPositiveDate, p.UniqueNumber ?? "");

        // --------- kľúče / sentinely ---------

        // interval podľa (district, dátum)
        public static SickByDistrictDate Low(int district, DateTime from) =>
            new SickByDistrictDate(null, district, from, string.Empty);

        public static SickByDistrictDate High(int district, DateTime toInclusive) =>
            new SickByDistrictDate(null, district, toInclusive, "\uFFFF");

        // interval pre konkrétnu osobu (district + person) – na hľadanie existujúceho záznamu
        public static SickByDistrictDate LowPerson(int district, string personId) =>
            new SickByDistrictDate(null, district, DateTime.MinValue, personId ?? "");

        public static SickByDistrictDate HighPerson(int district, string personId) =>
            new SickByDistrictDate(null, district, DateTime.MaxValue, personId ?? "");

        public int CompareTo(SickByDistrictDate other)
        {
            int c = NumberOfDistrict.CompareTo(other.NumberOfDistrict);
            if (c != 0) return c;

            c = LastPositiveDate.CompareTo(other.LastPositiveDate);
            if (c != 0) return c;

            return string.Compare(UniqueNumberPerson, other.UniqueNumberPerson, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => obj is SickByDistrictDate o && CompareTo(o) == 0;

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = 31 * h + NumberOfDistrict.GetHashCode();
                h = 31 * h + LastPositiveDate.GetHashCode();
                h = 31 * h + (UniqueNumberPerson?.GetHashCode() ?? 0);
                return h;
            }
        }

        public override string ToString() =>
            $"SickByDistrictDate(D{NumberOfDistrict}, {LastPositiveDate:yyyy-MM-dd HH:mm}, {UniqueNumberPerson})";
    }
}
