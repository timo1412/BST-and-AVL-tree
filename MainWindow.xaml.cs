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
            for (int i = 0; i < 4; i++) 
            {
                var p = new Person("First", "Last");

                idxById.Add(PersonByUniqueNumber.Of(p));
                idxByBirth.Add(PersonByBirth.Of(p));
            }



            var bst = new BST<Person>();
            StructurePerfTester.RunRandom(bst);
            Debug.WriteLine("BST report:\n" + StructurePerfTester.Run(bst));

            var avl = new AVLTree<Person>();
            StructurePerfTester.RunRandom(avl);
            Debug.WriteLine("AVL report:\n" + StructurePerfTester.Run(avl));
            Debug.WriteLine("KONIEC");
        }
    }
}