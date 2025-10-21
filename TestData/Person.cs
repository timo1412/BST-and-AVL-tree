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
        public string Name { get; }
        public string SeccondName { get; }   
        public double Weight { get; }
        public Person(string name, string seccondName)
        {
            Name = name;
            SeccondName = seccondName;
            Weight = Math.Round(rnd.NextDouble() * 100.0, 2, MidpointRounding.AwayFromZero);
        }
        public Person(string name, string seccondName, double weight)
        {
            Name = name;
            SeccondName = seccondName;
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
