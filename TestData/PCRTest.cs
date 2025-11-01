using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.TestData
{
    public class PCRTest
    {
        private static readonly Random Rnd = new Random();
        public DateTime DateStartTest { get; }
        public string UniqueNumberPerson { get; }
        public int UniqueNumberPCR { get; }
        public int UniqueNumberPCRPlace { get; }
        public int NumberOfDistrict { get; }
        public int NumberOfRegion { get; }
        public bool ResultOfTest { get; }
        public double ValueOfTest { get; }
        public string Note { get; }

        public PCRTest(
            DateTime dateStartTest,
            int numberOfDistrict,
            int numberOfRegion,
            bool resultOfTest,
            double valueOfTest,
            string note)
        {
            DateStartTest = dateStartTest;
            UniqueNumberPerson = RandomId(5); //TODO: spravit tak aby som pri pridavani testu vytvoril k nemu aj pacienta 
            UniqueNumberPCR = Rnd.Next(1, 1_000_000 + 1);
            UniqueNumberPCRPlace = Rnd.Next(1, 20_000 + 1);
            NumberOfDistrict = numberOfDistrict;
            NumberOfRegion = numberOfRegion;
            ResultOfTest = resultOfTest;
            ValueOfTest = valueOfTest;
            Note = note;
        }

        public PCRTest()
        {
            // Dátum: 2018-01-01 .. 2025-12-31
            var start = new DateTime(2018, 1, 1);
            var end = new DateTime(2025, 12, 31);
            int rangeDays = (end - start).Days;              // inkluzívny rozsah
            var date = start.AddDays(Rnd.Next(rangeDays + 1));

            // Čas: medzi 06:00 a 20:00 vrátane (minútový krok)
            int minutesOffset = Rnd.Next(0, 14 * 60 + 1);    // 0..840
            DateStartTest = date.Date.AddHours(6).AddMinutes(minutesOffset);

            // 5-znakové pacient ID (A-Z0-9)
            UniqueNumberPerson = RandomId(5);

            // 1..1_000_000
            UniqueNumberPCR = Rnd.Next(1, 1_000_000 + 1);

            // 1..20_000
            UniqueNumberPCRPlace = Rnd.Next(1, 20_000 + 1);

            // 1..79
            NumberOfDistrict = Rnd.Next(1, 79 + 1);

            // 1..8
            NumberOfRegion = Rnd.Next(1, 8 + 1);

            // 60:40 true:false
            ResultOfTest = Rnd.NextDouble() < 0.60;

            // 0.00 .. 1000.00, zaokrúhlené na 2 des. miesta
            ValueOfTest = Math.Round(Rnd.NextDouble() * 1000.0, 2, MidpointRounding.AwayFromZero);

            // default poznámka
            Note = "NOTE";
        }
        private static string RandomId(int length)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = alphabet[Rnd.Next(alphabet.Length)];
            return new string(chars);
        }
    }
}
