using SemestralnaPracaAUS2.Structures;
using SemestralnaPracaAUS2.TestData;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            var tree = new BST<Person>();
            bool isFind = true;
            int countDuplicate = 0;
            var findPerson = new Person(name: "John", seccondName: "Doe");
            List<Person> listPer = new List<Person>();
            for (int i = 0; i < 100000; i++)
            {
                var person = new Person(name: "John", seccondName: "Doe");                
                bool added = tree.Add(person);
                Console.WriteLine($"Add #{i + 1}: {person} -> {(added ? "OK" : "DUPLICATE")}");
                if (added) 
                {
                    listPer.Add(person);
                }
            }
            for (int i = 0; i < listPer.Count; i++)
            {
                if (tree.Find(listPer[i], out Person target))
                {
                    Console.WriteLine("Add");
                }
                else 
                { 
                    isFind = false; 
                }
            }
            
            MessageBox.Show("Vysledok: " + (countDuplicate + tree.count));
        }
    }
}