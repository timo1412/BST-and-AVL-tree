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
                    ShowTask2Form();   // << tu zobrazíme nový formulár
                    break;
                case 3:
                    ShowTask3Form();
                    break;
                case 4:
                    ShowTask4Form();
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
                case 22:
                    ShowRandomFillForm();
                    break;
                default:
                    try
                    {
                        // TODO: čítaj reálne hodnoty z GUI (TextBoxy/DatePickery)
                        _controller.SeedSystemFromGui(
                            persons: 10,
                            pcrPerPerson: 5,
                            dateFrom: new DateTime(2018, 1, 1),
                            dateTo: new DateTime(2025, 12, 31),
                            dayTimeFromText: "06:00",
                            dayTimeToText: "20:00",
                            positiveRatio: 0.60
                        );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void ShowRandomFillForm()
        {
            // Skry všetky ostatné panely, ktoré máš:
            HideAllTaskForms();

            if (randomFillForm != null)
                randomFillForm.Visibility = Visibility.Visible;
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
                int task = GetSelectedTaskIndex1Based();
                if (task == 0) throw new InvalidOperationException("Vyber operáciu v zozname.");

                switch (task)
                {
                    case 1:   // Insert PCR
                        HandleInsertPcr();
                        break;
                    case 2:
                        HandleFindPcrForPerson();
                        break;
                    case 3:
                        HandleListPcrForPerson();
                        break;
                    case 4:
                        HandleListPositiveByDistrictPeriod();
                        break;
                    case 22:  // Náhodné naplnenie systému
                        HandleSeedSystem();
                        break;

                    default:
                        MessageBox.Show("Táto operácia zatiaľ nie je implementovaná.", "Info",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
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
            if (task2Form != null) task2Form.Visibility = Visibility.Collapsed;
            if (task3Form != null) task3Form.Visibility = Visibility.Collapsed;
            if (task4Form != null) task4Form.Visibility = Visibility.Collapsed;
            if (randomFillForm != null) randomFillForm.Visibility = Visibility.Collapsed;
        }
        private void ShowTask4Form()
        {
            HideAllTaskForms();
            task4Form.Visibility = Visibility.Visible;
        }
        private void ShowTask3Form()
        {
            HideAllTaskForms();
            task3Form.Visibility = Visibility.Visible;
        }
        private void ShowTask2Form()
        {
            HideAllTaskForms();
            task2Form.Visibility = Visibility.Visible;
        }
        private void ShowTask1Form()
        {
            HideAllTaskForms();
            task1Form.Visibility = Visibility.Visible;
        }
        private void HandleListPositiveByDistrictPeriod() 
        {
            // nič neparsujem, nič nevalidujem – posúvam „surové“ vstupy do Controller-a
            var dateFrom = dpPosOkresFrom.SelectedDate;     // DateTime?
            var timeFrom = tbPosOkresFromTime.Text;         // string
            var dateTo = dpPosOkresTo.SelectedDate;       // DateTime?
            var timeTo = tbPosOkresToTime.Text;           // string
            var district = tbPosOkresDistrict.Text;         // string

            var tests = _controller.ListPositiveByDistrictFromGui(
                dateFrom, timeFrom,
                dateTo, timeTo,
                district
            );

            // zobrazíme výsledky (bez ďalšej logiky)
            dgPcrs.ItemsSource = tests;
            tabLists.SelectedIndex = 1; // prepni na tabu „PCR testy“

            txtStatus.Text = "Vyhľadávanie dokončené.";
        }
        private void HandleListPcrForPerson()
        {
            var personIdText = tbListPersonId.Text; // raw vstup

            var tests = _controller.ListPcrForPersonFromGui(personIdText);

            dgPcrs.ItemsSource = tests;   // zobraz do tabuľky PCR testy
            tabLists.SelectedIndex = 1;   // prepni na „PCR testy“

            txtStatus.Text = "Zobrazené PCR testy pre pacienta.";
            LogToGui($"[ListPCR-ByPerson] personId='{personIdText}' → loaded.");
        }
        private void HandleFindPcrForPerson()
        {
            var (person, pcr) = _controller.FindPcrForPersonFromGui(
                tbFindPersonId.Text,
                tbFindPcrCode.Text
            );

            if (person == null || pcr == null)
            {
                dgPersons.ItemsSource = null;
                dgPcrs.ItemsSource = null;
                txtStatus.Text = "Nenašli sa záznamy.";
                LogToGui($"[FindPCR] Nenájdené. personId='{tbFindPersonId.Text}', pcrCode='{tbFindPcrCode.Text}'.");
                return;
            }

            dgPersons.ItemsSource = new[] { person };
            dgPcrs.ItemsSource = new[] { pcr };
            txtStatus.Text = $"Nájdený test {pcr.UniqueNumberPCR} pre pacienta {person.UniqueNumber}.";
            LogToGui($"[FindPCR] OK Person={person.UniqueNumber}, PCR={pcr.UniqueNumberPCR} Date={pcr.DateStartTest:yyyy-MM-dd HH:mm}");
        }
        private void LogToGui(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} {message}";
            System.Diagnostics.Debug.WriteLine(line);

            if (txtConsole != null)
            {
                txtConsole.AppendText(line + Environment.NewLine);
                txtConsole.ScrollToEnd();
            }
        }
        private void BtnClearConsole_Click(object sender, RoutedEventArgs e)
        {
            txtConsole.Clear();
        }
        private void HandleInsertPcr()
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

            // voliteľné: reset polí
            // tbPcrTime.Clear(); tbDistrictCode.Clear(); tbRegionCode.Clear(); tbTestValue.Clear(); tbNote.Clear();
            // dpPcrDate.SelectedDate = null; chkResultPositive.IsChecked = false;
        }

        // 2) Samostatná obsluha pre „Náhodné naplnenie systému“
        private void HandleSeedSystem()
        {
            // POZOR: ak máš iné názvy prvkov, uprav tu:
            int persons = int.Parse(tbSeedPersons.Text);
            int pcrPerPerson = int.Parse(tbSeedPcrCount.Text);
            DateTime dateFrom = dpSeedFrom.SelectedDate ?? throw new InvalidOperationException("Zvoľ počiatočný dátum seeding-u.");
            DateTime dateTo = dpSeedTo.SelectedDate ?? throw new InvalidOperationException("Zvoľ koncový dátum seeding-u.");
            string dayFrom = tbSeedDayFrom.Text;   // "06:00"
            string dayTo = tbSeedDayTo.Text;     // "20:00"
            double positiveRatio = double.Parse(
                tbSeedPositiveRatio.Text.Replace(',', '.'),
                System.Globalization.CultureInfo.InvariantCulture
            );

            _controller.SeedSystemFromGui(
                persons: persons,
                pcrPerPerson: pcrPerPerson,
                dateFrom: dateFrom,
                dateTo: dateTo,
                dayTimeFromText: dayFrom,
                dayTimeToText: dayTo,
                positiveRatio: positiveRatio
            );
            // Controller už zobrazí MessageBox s výsledkom.
        }
    }
}