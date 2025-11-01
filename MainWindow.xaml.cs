using SemestralnaPracaAUS2.Architecture;
using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.Model;
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
        private readonly MyDatabaseModel _model;
        private readonly ApplicationView _view;
        private readonly ApplicationController _controller;
        public MainWindow()
        {
            InitializeComponent();
            _model = new MyDatabaseModel();
            _view = new ApplicationView(_model);
            _controller = new ApplicationController(_view);
        }
        private int GetSelectedTaskIndex1Based()
        {
            if (cmbOperacia == null) return 0;
            // SelectedIndex je 0-based, preto +1
            return cmbOperacia.SelectedIndex >= 0 ? cmbOperacia.SelectedIndex + 1 : 0;
        }

        private void cmbOperacia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int task = GetSelectedTaskIndex1Based();
            if (task == 0) return;

            switch (task)
            {
                case 1:
                    ShowTask1Form();
                    break;
                case 2:
                    // TODO: Vyhľadanie výsledku testu pre pacienta a zobrazenie všetkých údajov.
                    break;
                case 3:
                    // TODO: Výpis všetkých uskutočnených PCR testov pre daného pacienta (tooltip: usporiadané podľa dátumu a času).
                    break;
                case 4:
                    // TODO: Výpis všetkých pozitívnych testov za zadané obdobie pre zadaný okres.
                    break;
                case 5:
                    // TODO: Výpis všetkých testov za zadané obdobie pre zadaný okres.
                    break;
                case 6:
                    // TODO: Výpis všetkých pozitívnych testov za zadané obdobie pre zadaný kraj.
                    break;
                case 7:
                    // TODO: Výpis všetkých testov za zadané obdobie pre zadaný kraj.
                    break;
                case 8:
                    // TODO: Výpis všetkých pozitívnych testov za zadané obdobie.
                    break;
                case 9:
                    // TODO: Výpis všetkých testov za zadané obdobie.
                    break;
                case 10:
                    // TODO: Výpis chorých osôb v okrese k zadanému dátumu (X dní od pozitívneho testu).
                    break;
                case 11:
                    // TODO: Výpis chorých osôb v okrese k zadanému dátumu (X dní od pozitívneho testu), usporiadať podľa hodnoty testu.
                    break;
                case 12:
                    // TODO: Výpis chorých osôb v kraji k zadanému dátumu (X dní od pozitívneho testu).
                    break;
                case 13:
                    // TODO: Výpis chorých osôb k zadanému dátumu (X dní od pozitívneho testu).
                    break;
                case 14:
                    // TODO: Výpis jednej chorej osoby k zadanému dátumu z každého okresu s najvyššou hodnotou testu.
                    break;
                case 15:
                    // TODO: Výpis okresov usporiadaných podľa počtu chorých osôb k zadanému dátumu (X dní od pozitívneho testu).
                    break;
                case 16:
                    // TODO: Výpis krajov usporiadaných podľa počtu chorých osôb k zadanému dátumu (X dní od pozitívneho testu).
                    break;
                case 17:
                    // TODO: Výpis všetkých testov za zadané obdobie na danom pracovisku.
                    break;
                case 18:
                    // TODO: Vyhľadanie PCR testu podľa jeho kódu.
                    break;
                case 19:
                    // TODO: Vloženie osoby do systému.
                    break;
                case 20:
                    // TODO: Trvalé a nevratné vymazanie výsledku PCR testu podľa kódu.
                    break;
                case 21:
                    // TODO: Vymazanie osoby zo systému aj s jej výsledkami PCR testov.
                    break;
                default:
                    // Neplatný index (mimo 1..21)
                    break;
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
        //BtnNacitanieZoSuboru_Click  BtnUlozenieDoSuboru_Click
        private void BtnNahodneNaplnenie_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnNacitanieZoSuboru_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnUlozenieDoSuboru_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnProcessOperation(object sender, RoutedEventArgs e)
        {
            try
            {

                _controller.InsertPcrFromGui(
                    selectedDate: dpPcrDate.SelectedDate,
                    timeText: tbPcrTime.Text,
                    districtText: tbDistrictCode.Text,
                    regionText: tbRegionCode.Text,
                    resultPositive: chkResultPositive.IsChecked == true,
                    valueText: tbTestValue.Text,
                    note: tbNote.Text
                );

                MessageBox.Show("PCR test bol vložený.", "Hotovo",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // voliteľne: vyčistiť formulár
                // tbPcrTime.Clear(); tbDistrictCode.Clear(); tbRegionCode.Clear(); tbTestValue.Clear(); tbNote.Clear();
                // dpPcrDate.SelectedDate = null; chkResultPositive.IsChecked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void HideAllTaskForms()
        {
            if (task1Form != null) task1Form.Visibility = Visibility.Collapsed;
            // sem neskôr pridáš ďalšie: task2Form.Visibility = Collapsed; atď.
        }

        private void ShowTask1Form()
        {
            HideAllTaskForms();
            task1Form.Visibility = Visibility.Visible;
        }
    }
}