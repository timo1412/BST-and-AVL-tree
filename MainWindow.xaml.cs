using SemestralnaPracaAUS2.Structures;
using SemestralnaPracaAUS2.TestData;
using SemestralnaPracaAUS2.Tester;
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
            var bst = new BST<Person>();
            Debug.WriteLine("BST report:\n" + StructurePerfTester.Run(bst));

            var avl = new AVLTree<Person>();
            Debug.WriteLine("AVL report:\n" + StructurePerfTester.Run(avl));
            Debug.WriteLine("KONIEC");
        }


        private void  AVLTest() 
        {
            var avl = new AVLTree<Person>();
            avl.Add(new Person("first", "last", 50));
            avl.Add(new Person("first", "last", 30));
            avl.Add(new Person("first", "last", 70));
            avl.Add(new Person("first", "last", 20));
            avl.Add(new Person("first", "last", 40));
            avl.Add(new Person("first", "last", 10));
            var list = avl.LevelOrderList();
            foreach (var item in list) 
            {
                Debug.WriteLine(item.Weight);
            }
        }

        private void TestBST() 
        {
            for (int j = 0;j<10 ;j++) 
            {
                Random rnd = new Random(j);
                var tree = new BST<Person>();
                var listInserted = new List<Person>();
                for (int i = 0; i < 10000000; i++)
                {
                    double operacia = rnd.NextDouble();
                    if (operacia < 0.3)
                    {
                        if (listInserted.Count > 0)
                        {
                            int index = rnd.Next(listInserted.Count); // 0 <= index < Count
                            var delete = listInserted[index];
                            Person find;
                            tree.Delete(delete);
                            listInserted.RemoveAt(index);
                            bool afterFind = tree.Find(delete, out find);
                            if (afterFind) 
                            {
                                Debug.WriteLine("nemoze nastat DELETE");
                            }
                        }
                    }
                    else if (operacia < 0.5)
                    {
                        if (listInserted.Count != 0)
                        {
                            int index = rnd.Next(listInserted.Count); // 0 <= index < Count
                            Person found;
                            var find = tree.Find(listInserted[index], out found);
                            if (!find) 
                            {
                                Debug.WriteLine("nemoze nastat FIND");
                            }
                        }
                    }
                    else
                    {
                        double key = Math.Round(rnd.NextDouble() * 10000.0, 2, MidpointRounding.AwayFromZero);
                        var person = new Person("first", "last", key);
                        var isInserted = tree.Add(person);
                        if(isInserted)
                            listInserted.Add(person);
                    }
                }
            }
            
            Debug.WriteLine("");
        }
    }
}