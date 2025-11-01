using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.Structures;
using SemestralnaPracaAUS2.TestData;
using SemestralnaPracaAUS2.Tester;
using SemestralnaPracaAUS2.Wrappers;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace SemestralnaPracaAUS2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var idxById = new AVLTree<PersonByUniqueNumber>();
            var idxByBirth = new AVLTree<PersonByBirth>();
            var idxByCode = new AVLTree<PcrByCode>();
            var idxByPerson = new AVLTree<PcrByPersonDate>();
            var idxByDate = new AVLTree<PcrByDate>();
            var idxByRegionDate = new AVLTree<PcrByRegionDate>();
            var idxByDistrictDate = new AVLTree<PcrByDistrictDate>();
            var idxByWorkplaceDate = new AVLTree<PcrByWorkplaceDate>();
            var list = new List<PCRTest>();
            for (int i = 0; i < 4; i++) 
            {
                var p = new Person("First", "Last");
                var pcr = new PCRTest();
                list.Add(pcr);
                idxByWorkplaceDate.Add(PcrByWorkplaceDate.Of(pcr));
                idxByDistrictDate.Add(PcrByDistrictDate.Of(pcr));
                idxByRegionDate.Add(PcrByRegionDate.Of(pcr));
                idxByDate.Add(PcrByDate.Of(pcr));
                idxByPerson.Add(PcrByPersonDate.Of(pcr));
                idxByCode.Add(PcrByCode.Of(pcr));
                //idxById.Add(PersonByUniqueNumber.Of(p));
                //idxByBirth.Add(PersonByBirth.Of(p));
            }
            Debug.WriteLine("=== RAW PCR tests (as generated) ===");
            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                Debug.WriteLine(
                    $"[{i}] dt={t.DateStartTest:yyyy-MM-dd HH:mm}, " +
                    $"code={t.UniqueNumberPCR}, person={t.UniqueNumberPerson}, " +
                    $"region={t.NumberOfRegion}, district={t.NumberOfDistrict}, workplace={t.UniqueNumberPCRPlace}, " +
                    $"result={(t.ResultOfTest ? "POS" : "NEG")}, value={t.ValueOfTest:F2}");
            }

            DumpLevelOrder("PcrByCode           (key = UniqueNumberPCR)",
    idxByCode,
    w => $"{w.UniqueNumberPCR}");

            DumpLevelOrder("PcrByPersonDate     (key = personId, date, code)",
                idxByPerson,
                w => $"{w.UniqueNumberPerson} | {w.DateStartTest:yyyy-MM-dd HH:mm} | {w.UniqueNumberPCR}");

            DumpLevelOrder("PcrByDate           (key = date, code, personId)",
                idxByDate,
                w => $"{w.DateStartTest:yyyy-MM-dd HH:mm} | {w.UniqueNumberPCR} | {w.UniqueNumberPerson}");

            DumpLevelOrder("PcrByRegionDate     (key = region, date, code)",
                idxByRegionDate,
                w => $"R{w.NumberOfRegion} | {w.DateStartTest:yyyy-MM-dd HH:mm} | {w.UniqueNumberPCR}");

            DumpLevelOrder("PcrByDistrictDate   (key = district, date, code)",
                idxByDistrictDate,
                w => $"D{w.NumberOfDistrict} | {w.DateStartTest:yyyy-MM-dd HH:mm} | {w.UniqueNumberPCR}");

            DumpLevelOrder("PcrByWorkplaceDate  (key = workplace, date, code)",
                idxByWorkplaceDate,
                w => $"W{w.UniqueNumberPCRPlace} | {w.DateStartTest:yyyy-MM-dd HH:mm} | {w.UniqueNumberPCR}");


            var bst = new BST<Person>();
            StructurePerfTester.RunRandom(bst);
            Debug.WriteLine("BST report:\n" + StructurePerfTester.Run(bst));

            var avl = new AVLTree<Person>();
            StructurePerfTester.RunRandom(avl);
            Debug.WriteLine("AVL report:\n" + StructurePerfTester.Run(avl));
            Debug.WriteLine("KONIEC");
        }
        void DumpLevelOrder<T>(string title, AVLTree<T> tree, Func<T, string> keySel) where T : IMyComparable<T>
        {
            Debug.WriteLine($"\n=== LevelOrder: {title} ===");
            foreach (var w in tree.LevelOrder())
                Debug.WriteLine(keySel(w));
        }
    }
}