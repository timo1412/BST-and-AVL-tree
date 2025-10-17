using SemestralnaPracaAUS2.Structures;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Diagnostics;
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
            TestBST();
            var tree = new BST<Person>();
            tree.Add(new Person("first", "last", 8));
            tree.Add(new Person("first", "last", 4));
            tree.Add(new Person("first", "last", 12));
            tree.Add(new Person("first", "last", 15));
            var list = tree.LevelOrderList();
            foreach (var item in list) 
            {
                Debug.WriteLine(item.Weight);
            }
            tree.Delete(new Person("first", "last", 8));
            var listDel = tree.LevelOrderList();
            Debug.WriteLine("================================");
            foreach (var item in listDel)
            {
                Debug.WriteLine(item.Weight);
            }
            Debug.WriteLine("================================");
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