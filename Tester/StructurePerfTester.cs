using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.Structures;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Tester
{
    public static class StructurePerfTester
    {
        const int INSERT_COUNT = 10_000_000;
        const int DELETE_COUNT = 2_000_000;
        const int POINT_FIND_COUNT = 5_000_000;
        const int INTERVAL_FIND_COUNT = 1_000_000;
        const int MIN_OPS = 2_000_000;
        const int MAX_OPS = 2_000_000;

        private static readonly Random Rnd = new(123456);

        public sealed class Report
        {
            public TimeSpan InsertTime, DeleteTime, PointFindTime, IntervalFindTime, MinTime, MaxTime;
            public long CountAfter;
            public override string ToString() =>
                                        $@"Insert:          {InsertTime}
                                        Delete:          {DeleteTime}
                                        Find (point):    {PointFindTime}
                                        Find (interval): {IntervalFindTime}
                                        Min():           {MinTime}
                                        Max():           {MaxTime}
                                        Count after:     {CountAfter}";
        }

        /// <summary>
        /// Otestuje tvoj BST<Person> alebo AVLTree<Person>.
        /// Porovnávacím kľúčom je Person.Weight (double).
        /// Aby sme sa vyhli duplicitám, generujeme unikátne 'weight' ako Double z unikátneho int kľúča.
        /// </summary>
        public static Report Run(BST<Person> tree)
        {
            // -------- 1) INSERT 10M unikátnych prvkov --------
            var inserted = new List<Person>(INSERT_COUNT);
            var usedKeys = new HashSet<int>();
            var sw = Stopwatch.StartNew();

            while (inserted.Count < INSERT_COUNT)
            {
                // Unikátny celoint kľúč -> prekonvertujeme na double (bez zaokrúhľovania)
                int k = Rnd.Next(int.MinValue, int.MaxValue);
                if (!usedKeys.Add(k)) continue;

                double weight = k; // unikátny a totálne rozlíšiteľný
                var p = new Person("N" + k, "S" + k, weight);

                if (tree.Add(p))
                {
                    inserted.Add(p);
                }
                else
                {
                    // (nemalo by sa stať, ale pre istotu vrátime k späť)
                    usedKeys.Remove(k);
                }
            }

            sw.Stop();
            var insertTime = sw.Elapsed;

            // Pre ďalšie fázy potrebujeme rýchly prístup k náhodným existujúcim prvkom
            // a aj zoradený snapshot kvôli intervalom (≥500 hitov).
            // Zoradíme podľa Person.CompareTo (t.j. podľa Weight).
            inserted.Sort((a, b) => a.CompareTo(b));

            // Už teraz si uchovajme extrémy (min/max) – využijeme pri Min/Max meraní, ak strom nemá API na Min/Max.
            // (Tvoje zadanie pýta aj Min/Max operácie; ak nemáš TryGetMin/TryGetMax v strome, použijeme Find(min) / Find(max) ako náhradu.)
            var minPerson = inserted[0];
            var maxPerson = inserted[^1];

            // -------- 2) DELETE 2M náhodných existujúcich prvkov --------
            sw.Restart();
            for (int i = 0; i < DELETE_COUNT; i++)
            {
                int idx = Rnd.Next(inserted.Count);
                var key = inserted[idx];
                if (tree.Delete(key))
                {
                    // odstráň aj z lokálneho zoznamu (swap-remove)
                    inserted[idx] = inserted[^1];
                    inserted.RemoveAt(inserted.Count - 1);
                }
                else
                {
                    // ak by delete nevyšiel (nemalo by), skús iný
                    i--;
                }
            }
            sw.Stop();
            var deleteTime = sw.Elapsed;

            // Pre intervaly potrebujeme znovu zoradené (po mazaní).
            inserted.Sort((a, b) => a.CompareTo(b));

            // -------- 3) POINT FIND 5M náhodných existujúcich --------
            sw.Restart();
            for (int i = 0; i < POINT_FIND_COUNT; i++)
            {
                int idx = Rnd.Next(inserted.Count);
                var key = inserted[idx];
                if (!tree.Find(key, out _))
                    throw new InvalidOperationException("Point Find: prvok mal byť v štruktúre.");
            }
            sw.Stop();
            var pointFindTime = sw.Elapsed;

            // -------- 4) INTERVAL FIND 1M dotazov (≥ 500 výsledkov každý) --------
            // Garantujeme ≥500 tak, že vyberieme náhodný začiatok a koniec = start + 499 v zoradenom zozname.
            // Potom spravíme Range([low..high]) – tvoje BST/AVL vracia IEnumerable v poradí inorder. :contentReference[oaicite:2]{index=2}
            int n = inserted.Count;
            if (n < 600) throw new InvalidOperationException("Na intervalové testy treba mať v štruktúre aspoň ~600 prvkov.");
            sw.Restart();
            for (int q = 0; q < INTERVAL_FIND_COUNT; q++)
            {
                int startIdx = Rnd.Next(0, n - 500);
                int endIdx = startIdx + 499;
                var low = inserted[startIdx];
                var high = inserted[endIdx];

                int found = 0;
                foreach (var _ in tree.Range(low, high))
                    found++;

                if (found < 500)
                    throw new InvalidOperationException($"Interval Find: očakávaných ≥500, našiel som {found}.");
            }
            sw.Stop();
            var intervalFindTime = sw.Elapsed;

            // -------- 5) MIN a MAX (používajú tvoje FindMin/FindMax) --------
            sw.Restart();
            for (int i = 0; i < MIN_OPS; i++)
            {
                if (!tree.FindMin(out _))
                    throw new InvalidOperationException("Min: štruktúra je prázdna?");
            }
            sw.Stop();
            var minTime = sw.Elapsed;

            sw.Restart();
            for (int i = 0; i < MAX_OPS; i++)
            {
                if (!tree.FindMax(out _))
                    throw new InvalidOperationException("Max: štruktúra je prázdna?");
            }
            sw.Stop();
            var maxTime = sw.Elapsed;

            return new Report
            {
                InsertTime = insertTime,
                DeleteTime = deleteTime,
                PointFindTime = pointFindTime,
                IntervalFindTime = intervalFindTime,
                MinTime = minTime,
                MaxTime = maxTime,
                CountAfter = tree.count
            };
        }
    }
}
