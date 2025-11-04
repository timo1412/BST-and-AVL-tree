using SemestralnaPracaAUS2.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.TestData
{
    public class Person : IMyComparable<Person>
    {
        private static readonly Random rnd = new Random();
        public string FirstName { get; }
        public string LastName { get; }   
        public double Weight { get; }
        public string UniqueNumber { get; }
        private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public DateTime Birth { get; }
        public Person(string name, string seccondName)
        {
            FirstName = name;
            LastName = seccondName;
            Weight = Math.Round(rnd.NextDouble() * 100.0, 2, MidpointRounding.AwayFromZero);
            DateTime minDate = new DateTime(1900, 1, 1);
            DateTime maxDate = new DateTime(2025, 12, 31); 
            TimeSpan dateRange = maxDate - minDate;
            int totalDays = (int)dateRange.TotalDays;
            Birth = minDate.AddDays(rnd.Next(totalDays + 1));
            UniqueNumber = GenerateRandomString(6);
        }
        public Person(string firstName, string lastName, double weight, DateTime birth, string uniqueNumber)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Weight = weight;
            Birth = birth;
            UniqueNumber = uniqueNumber ?? throw new ArgumentNullException(nameof(uniqueNumber));
        }
        private static string GenerateRandomString(int length)
        {
            // Používame StringBuilder pre efektívne spájanie znakov
            var result = new System.Text.StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                // Vyber náhodný index z povolených znakov
                int randomIndex = rnd.Next(AllowedChars.Length);

                // Pripoj náhodný znak k výsledku
                result.Append(AllowedChars[randomIndex]);
            }

            return result.ToString();
        }
        public Person(string name, string seccondName, double weight)
        {
            FirstName = name;
            LastName = seccondName;
            Weight = weight;
        }
        public int CompareTo(Person other)
        {
            if (other is null) return 1;

            int cmp = Weight.CompareTo(other.Weight);
            if (cmp < 0) return -1;
            if (cmp > 0) return 1;
            return 0;
        }
    }
}
