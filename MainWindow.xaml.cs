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
            // Sem daj, čo sa má vykonať po kliknutí
            MessageBox.Show("Test tlačidlo kliknuté!");
            // napr. spustiť výpočet, otvoriť okno, zavolať službu, atď.
        }

    }
}