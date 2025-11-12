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
        public static void RunAddFindDeleteBalanced(BST<Person> tree,int operations = 1_000_000,double findShare = 0.20,double deleteHitShare = 0.80) 
        {
            int finds = (int)Math.Round(operations * findShare);
            int adds = (operations - finds) / 2;
            int dels = operations - finds - adds; 
            if (dels != adds) dels = adds;         

            // 2) Priprav náhodne premiešanú sekvenciu operácií: 0=ADD, 1=DEL, 2=FIND
            var ops = new int[operations];
            int k = 0;
            for (int i = 0; i < adds; i++) ops[k++] = 0;
            for (int i = 0; i < dels; i++) ops[k++] = 1;
            for (int i = 0; i < finds; i++) ops[k++] = 2;
            for (int i = ops.Length - 1; i > 0; i--) { int j = Rnd.Next(i + 1); (ops[i], ops[j]) = (ops[j], ops[i]); }

            // 3) Jednoduchý unikátny generátor kľúčov pre Person (Weight je komparátor)
            long nextKey = 0;
            var inserted = new List<Person>(adds);

            // 4) Spúšťaj operácie bez špeciálneho „ak prázdny, tak vlož“ správania
            for (int i = 0; i < ops.Length; i++)
            {
                switch (ops[i])
                {
                    case 0:
                        {
                            int key = unchecked((int)nextKey++);
                            var p = new Person("N" + key, "S" + key, (double)key);
                            if (tree.Add(p)) inserted.Add(p);
                            break;
                        }

                    case 1:
                        {
                            if (Rnd.NextDouble() < deleteHitShare && inserted.Count > 0)
                            {
                                int ei = Rnd.Next(inserted.Count);
                                var victim = inserted[ei];
                                if (tree.Delete(victim))
                                {
                                    inserted[ei] = inserted[^1];
                                    inserted.RemoveAt(inserted.Count - 1);
                                }
                            }
                            else
                            {
                                int rk = Rnd.Next(int.MinValue, int.MaxValue);
                                var ghost = new Person("X" + rk, "Y" + rk, (double)rk);
                                tree.Delete(ghost);
                            }
                            break;
                        }

                    default:
                        {
                            if (Rnd.Next(2) == 0 && inserted.Count > 0)
                            {
                                var probe = inserted[Rnd.Next(inserted.Count)];
                                tree.Find(probe, out _);
                            }
                            else
                            {
                                int fk = Rnd.Next(int.MinValue, int.MaxValue);
                                var probe = new Person("F" + fk, "Z" + fk, (double)fk);
                                tree.Find(probe, out _);
                            }
                            break;
                        }
                }
            }
        }
        public static void RunRandom(BST<Person> tree) 
        {
            int countOp = 1000000;
            double insert = 0.4;
            double delete =  0.3;
            double find = 0.1;
            double intFind = 0.2;
            var inserted = new List<Person>();
            var foundInt = new List<Person>();
            for (int i = 0; i < countOp; i++) 
            {
                double operation = Rnd.NextDouble();
                if (operation < insert)
                {
                    int k = Rnd.Next(int.MinValue, int.MaxValue);

                    double weight = k;
                    var p = new Person("N" + k, "S" + k, weight);

                    if (tree.Add(p))
                    {
                        inserted.Add(p);
                    }
                }
                else if (operation < insert + delete)
                {
                    if (inserted.Count > 0)
                    {
                        int idx = Rnd.Next(inserted.Count);
                        var key = inserted[idx];
                        if (tree.Delete(key))
                        {
                            inserted[idx] = inserted[^1];
                            inserted.RemoveAt(inserted.Count - 1);
                        }
                    }
                    else 
                    {
                        var key = new Person("c", "f");
                        if (tree.Delete(key))
                        {
                            throw new InvalidOperationException("Nemoze nastat lebo hladam nieco co tam nie je");
                        }
                    }
                    
                }
                else if (operation < insert + delete + find)
                {
                    Person findPer = null;
                    if (inserted.Count != 0)
                    {
                        int idx = Rnd.Next(inserted.Count);
                        findPer = inserted[idx];
                        if (!tree.Find(findPer, out _))
                            Debug.WriteLine("Prvok sa ma nachadzat v strukture");
                    }
                    else
                    {
                        findPer = new Person("", "");
                        if (tree.Find(findPer, out _))
                            Debug.WriteLine("Prvok sa nema nachadzat v strukture");
                    }
                }
                else if (operation < intFind + insert + delete + find) 
                {
                    inserted.Sort((a, b) => a.CompareTo(b));
                    List<Person> interval = new List<Person>();
                    int startIdx = 0;
                    int endIdx = 0;
                    if (inserted.Count > 2) 
                    {
                        int n = inserted.Count;
                        startIdx = Rnd.Next(0, n);
                        endIdx = Rnd.Next(startIdx, n);
                        interval = inserted.GetRange(startIdx, endIdx);
                    }
                    var low = inserted.Count == 0 ? new Person("f",":") : inserted[startIdx];
                    var high = inserted.Count == 0 ? new Person("f", ":") : inserted[endIdx]; 

                    int found = 0;
                    foreach (var foundIntItem in tree.Range(low, high)) 
                    {
                        found++;
                        foundInt.Add(foundIntItem);
                    }
                    for (int j = 0; i < interval.Count; i++)
                    {
                        if (interval[i].CompareTo(foundInt[i]) != 0) 
                        {
                            throw new InvalidOperationException("Nemoze nastat lebo");
                        }
                    }
                }
            }
        }
        public static Report Run(BST<Person> tree, bool incrementalKeys)
        {
            // -------- 1) INSERT 10M unikátnych prvkov --------
            var inserted = new List<Person>(INSERT_COUNT);
            //var usedKeys = new HashSet<int>();
            var sw = Stopwatch.StartNew();

            if (incrementalKeys)
            {
                // inkrementálne kľúče: 0,1,2,...
                int nextKey = 0;
                while (inserted.Count < INSERT_COUNT)
                {
                    int k = nextKey++;
                    double weight = k;
                    var p = new Person("N" + k, "S" + k, weight);

                    if (tree.Add(p))
                    {
                        inserted.Add(p);
                    }
                    // pri inkrementálnych kľúčoch sa kolízia neočakáva
                }
            }
            else
            {
                // náhodné unikátne kľúče (pôvodné správanie)
                var usedKeys = new HashSet<int>();
                while (inserted.Count < INSERT_COUNT)
                {
                    int k = Rnd.Next(int.MinValue, int.MaxValue);
                    if (!usedKeys.Add(k)) continue;

                    double weight = k; // unikátny a jednoznačný
                    var p = new Person("N" + k, "S" + k, weight);

                    if (tree.Add(p))
                    {
                        inserted.Add(p);
                    }
                    else
                    {
                        // (nemalo by sa stať, ale pre istotu kľúč uvoľníme)
                        usedKeys.Remove(k);
                    }
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
