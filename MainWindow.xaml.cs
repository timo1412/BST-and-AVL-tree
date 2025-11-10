using Microsoft.Win32;
using SemestralnaPracaAUS2.Architecture;
using SemestralnaPracaAUS2.Interface;
using SemestralnaPracaAUS2.Model;
using SemestralnaPracaAUS2.Structures;
using SemestralnaPracaAUS2.TestData;
using SemestralnaPracaAUS2.Tester;
using SemestralnaPracaAUS2.Wrappers;
using System;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Globalization;
using System.IO;
using System.IO;
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
        private readonly ApplicationView _view;
        public MainWindow()
        {
            InitializeComponent();
            _view = new ApplicationView();
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
                    ShowTask5Form();
                    break;
                case 6:
                    ShowTask6Form();
                    break;
                case 7:
                    ShowTask7Form();
                    break;
                case 8:
                    ShowTask8Form();
                    break;
                case 9:
                    ShowTask9Form();
                    break;
                case 10:
                    ShowTask10Form();
                    break;
                case 11:
                    ShowTask11Form();
                    break;
                case 12:
                    ShowTask12Form();
                    break;
                case 13:
                    ShowTask13Form();
                    break;
                case 14:
                    ShowTask14Form();
                    break;
                case 15:
                    ShowTask15Form();
                    break;
                case 16:
                    ShowTask16Form();
                    break;
                case 17:
                    ShowTask17Form();
                    break;
                case 18:
                    ShowTask18Form();
                    break;
                case 19:
                    ShowTask19Form();
                    break;
                case 20:
                    ShowTask20Form();
                    break;
                case 21:
                    ShowTask21Form();
                    break;
                case 22:
                    ShowRandomFillForm();
                    break;
                default:
                    try
                    {
                        // TODO: čítaj reálne hodnoty z GUI (TextBoxy/DatePickery)
                        _view.SeedSystemFromGui(
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
            try
            {
                var root = AppDomain.CurrentDomain.BaseDirectory;

                var ofd = new OpenFileDialog
                {
                    Title = "Vyber ľubovoľný súbor z priečinka, z ktorého chceš načítať",
                    InitialDirectory = root,
                    CheckFileExists = true,
                    Multiselect = false,
                    Filter = "Všetky súbory|*.*"
                };

                if (ofd.ShowDialog() == true)
                {
                    var folder = System.IO.Path.GetDirectoryName(ofd.FileName)!;

                    var (persons, tests) = _view.LoadAllFromFolderFromGui(folder, clearExisting: true);

                    txtStatus.Text = $"Načítané osoby: {persons}, testy: {tests} z „{folder}“.";
                    LogToGui($"[LOAD] OK persons={persons}, tests={tests}, folder='{folder}'");
                }
                else
                {
                    txtStatus.Text = "Načítanie zrušené používateľom.";
                    LogToGui("[LOAD] canceled");
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Chyba pri načítaní dát zo súborov.";
                LogToGui($"[LOAD] ERROR: {ex.Message}");
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnUlozenieDoSuboru_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = _view.SaveAllToFileInAppRoot();
                txtStatus.Text = $"Dáta uložené do súboru: {path}";
                LogToGui($"[SAVE] OK -> {path}");
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Chyba pri ukladaní do súboru.";
                LogToGui($"[SAVE] ERROR: {ex.Message}");
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnProcessOperation(object sender, RoutedEventArgs e)
        {
            try
            {
                int task = GetSelectedTaskIndex1Based();
                if (task == 0) throw new InvalidOperationException("Vyber operáciu v zozname.");

                switch (task)
                {
                    case 1:
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
                    case 5:
                        HandleListAllByDistrictPeriod();
                        break;
                    case 6:
                        HandleListPositiveByRegionPeriod();
                        break;
                    case 7:
                        HandleListAllByRegionPeriod();
                        break;
                    case 8:
                        HandleListPositiveAllPeriod();
                        break;
                    case 9:
                        HandleListAllPeriod();
                        break;
                    case 10:
                        HandleListSickByDistrictAtDate();
                        break;
                    case 11:
                        HandleListSickByDistrictAtDateSorted();
                        break;
                    case 12:
                        HandleListSickByRegionAtDate();
                        break;
                    case 13:
                        HandleListSickAllAtDate();
                        break;
                    case 14:
                        HandleListTopSickPerDistrictAtDate();
                        break;
                    case 15:
                        HandleListDistrictsBySickCountAtDate();
                        break;
                    case 16:
                        HandleListRegionsBySickCountAtDate();
                        break;
                    case 17:
                        HandleListAllByWorkplacePeriod();
                        break;
                    case 18:
                        HandleFindPcrByCode();
                        break;
                    case 19:
                        HandleAddPerson();
                        break;
                    case 20:
                        HandleDeletePcrByCode();
                        break;
                    case 21:
                        HandleDeletePersonWithTests();
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
            if (task5Form != null) task5Form.Visibility = Visibility.Collapsed;
            if (task6Form != null) task6Form.Visibility = Visibility.Collapsed;
            if (task7Form != null) task7Form.Visibility = Visibility.Collapsed;
            if (task8Form != null) task8Form.Visibility = Visibility.Collapsed;
            if (task9Form != null) task9Form.Visibility = Visibility.Collapsed;
            if (task10Form != null) task10Form.Visibility = Visibility.Collapsed;
            if (task11Form != null) task11Form.Visibility = Visibility.Collapsed;
            if (task12Form != null) task12Form.Visibility = Visibility.Collapsed;
            if (task13Form != null) task13Form.Visibility = Visibility.Collapsed;
            if (task14Form != null) task14Form.Visibility = Visibility.Collapsed;
            if (task15Form != null) task15Form.Visibility = Visibility.Collapsed;
            if (task16Form != null) task16Form.Visibility = Visibility.Collapsed;
            if (task17Form != null) task17Form.Visibility = Visibility.Collapsed;
            if (task18Form != null) task18Form.Visibility = Visibility.Collapsed;
            if (task19Form != null) task19Form.Visibility = Visibility.Collapsed;
            if (task20Form != null) task20Form.Visibility = Visibility.Collapsed;
            if (task21Form != null) task21Form.Visibility = Visibility.Collapsed;
            if (randomFillForm != null) randomFillForm.Visibility = Visibility.Collapsed;
        }
        private void ShowTask14Form()
        {
            HideAllTaskForms();
            task14Form.Visibility = Visibility.Visible;
        }
        private void ShowTask13Form()
        {
            HideAllTaskForms();
            task13Form.Visibility = Visibility.Visible;
        }
        private void ShowTask12Form()
        {
            HideAllTaskForms();
            task12Form.Visibility = Visibility.Visible;
        }
        private void ShowTask15Form()
        {
            HideAllTaskForms();
            task15Form.Visibility = Visibility.Visible;
        }
        private void ShowTask16Form()
        {
            HideAllTaskForms();
            task16Form.Visibility = Visibility.Visible;
        }
        private void ShowTask21Form()
        {
            HideAllTaskForms();
            task21Form.Visibility = Visibility.Visible;
        }
        private void ShowTask20Form()
        {
            HideAllTaskForms();
            task20Form.Visibility = Visibility.Visible;
        }
        private void ShowTask19Form()
        {
            HideAllTaskForms();
            task19Form.Visibility = Visibility.Visible;
        }
        private void ShowTask18Form()
        {
            HideAllTaskForms();
            task18Form.Visibility = Visibility.Visible;
        }
        private void ShowTask17Form()
        {
            HideAllTaskForms();
            task17Form.Visibility = Visibility.Visible;
        }
        private void ShowTask11Form()
        {
            HideAllTaskForms();
            task11Form.Visibility = Visibility.Visible;
        }
        private void ShowTask10Form()
        {
            HideAllTaskForms();
            task10Form.Visibility = Visibility.Visible;
        }
        private void ShowTask9Form()
        {
            HideAllTaskForms();
            task9Form.Visibility = Visibility.Visible;
        }
        private void ShowTask8Form()
        {
            HideAllTaskForms();
            task8Form.Visibility = Visibility.Visible;
        }
        private void ShowTask7Form()
        {
            HideAllTaskForms();
            task7Form.Visibility = Visibility.Visible;
        }
        private void ShowTask6Form()
        {
            HideAllTaskForms();
            task6Form.Visibility = Visibility.Visible;
        }
        private void ShowTask5Form()
        {
            HideAllTaskForms();
            task5Form.Visibility = Visibility.Visible;
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
        private void HandleDeletePersonWithTests()
        {
            var personIdText = tbDeletePersonId.Text; // raw vstup

            var ok = _view.DeletePersonWithTestsFromGui(personIdText);

            // UI reakcia – po zmazaní vyčisti tabuľky
            dgPersons.ItemsSource = null;
            dgPcrs.ItemsSource = null;

            if (ok)
            {
                txtStatus.Text = $"Osoba (ID/RČ: {personIdText}) bola trvalo vymazaná vrátane všetkých PCR testov.";
                LogToGui($"[DeletePerson] OK personId='{personIdText}' -> ALL TESTS REMOVED");
            }
            else
            {
                txtStatus.Text = "Osobu sa nepodarilo vymazať.";
                LogToGui($"[DeletePerson] FAIL personId='{personIdText}'");
            }
        }
        private void HandleDeletePcrByCode()
        {
            var pcrCodeText = tbDeletePcrCode.Text; // raw vstup

            var deleted = _view.DeletePcrByCodeFromGui(pcrCodeText);

            // voliteľne ukáž zmazaný záznam vpravo (ako „potvrdenku“)
            dgPcrs.ItemsSource = new[] { deleted };
            tabLists.SelectedIndex = 1; // „PCR testy“

            txtStatus.Text = $"Test {deleted.UniqueNumberPCR} bol trvalo vymazaný.";
            LogToGui($"[DeletePCR] code='{pcrCodeText}' → DELETED (id={deleted.UniqueNumberPCR})");
        }
        private void HandleAddPerson()
        {
            var firstName = tbAddFirstName.Text;
            var lastName = tbAddLastName.Text;
            var birthDate = dpAddBirth.SelectedDate; // DateTime?

            var person = _view.AddPersonFromGui(firstName, lastName, birthDate);

            // zobraz novú osobu v tabuľke Osoby
            ConfigurePersonsGridForPerson();
            dgPersons.ItemsSource = new[] { person };
            tabLists.SelectedIndex = 0;

            txtStatus.Text = $"Osoba vytvorená (ID: {person.UniqueNumber}).";
            LogToGui($"[AddPerson] OK -> {person.UniqueNumber} {person.FirstName} {person.LastName} ({person.Birth:yyyy-MM-dd})");
        }
        private void HandleFindPcrByCode()
        {
            var pcrCodeText = tbFindOnlyPcrCode.Text; // raw vstup z GUI

            var test = _view.FindPcrByCodeFromGui(pcrCodeText);

            // zobraz test v pravej tabuľke
            dgPcrs.ItemsSource = new[] { test };
            tabLists.SelectedIndex = 1; // prepni na „PCR testy“

            txtStatus.Text = $"Zobrazený PCR test {test.UniqueNumberPCR}.";
            LogToGui($"[FindPCR-ByCode] code='{pcrCodeText}' -> {test.UniqueNumberPCR}");
        }
        private void HandleListSickByDistrictAtDateSorted()
        {
            var atDate = dpSickOkresSortedDate.SelectedDate; // DateTime?
            var atTime = tbSickOkresSortedTime.Text;         // string
            var district = tbSickOkresSortedDistrict.Text;     // string
            var xDays = tbSickOkresSortedXDays.Text;        // string

            // Controller vráti dvojice (Person, PCRTest), už ZORADENÉ podľa hodnoty testu
            var rows = _view.ListSickByDistrictAtDateSortedWithTestFromGui(
                atDate, atTime, district, xDays);
            ConfigurePersonsGridForSickWithTest();
            dgPersons.ItemsSource = rows;     // << dáme do tabuľky „Osoby“
            tabLists.SelectedIndex = 0;       // prepni na Osoby
            txtStatus.Text = "Chorí v okrese (zoradení podľa hodnoty testu) s referenčným testom.";
            LogToGui($"[SickByDistrictSorted] okres='{district}', at={atDate:yyyy-MM-dd} {atTime}, X={xDays}");
        }
        public sealed class DistrictSickRow
        {
            public int District { get; init; }
            public int SickCount { get; init; }
        }
        private void HandleListRegionsBySickCountAtDate()
        {
            var atDate = dpSickRegionsDate.SelectedDate;
            var atTime = tbSickRegionsTime.Text;
            var xDays = tbSickRegionsXDays.Text;

            var rows = _view.ListRegionsBySickCountAtDateFromGui(atDate, atTime, xDays);

            dgRegions.ItemsSource = null;
            dgRegions.Items.Clear();

            dgRegions.ItemsSource = rows
                .Select(t => Tuple.Create(t.Region, t.SickCount))
                .ToList();

            tabLists.SelectedIndex = 2;
            txtStatus.Text = "Zobrazené kraje podľa počtu chorých osôb k dátumu.";
            LogToGui($"[RegionsBySickCount] at={atDate:yyyy-MM-dd} {atTime}, X={xDays}");
        }
        private void HandleListSickByRegionAtDate()
        {
            var atDate = dpSickRegionDate.SelectedDate; // DateTime?
            var atTime = tbSickRegionTime.Text;         // string
            var region = tbSickRegionCode.Text;         // string
            var xDays = tbSickRegionXDays.Text;        // string

            var persons = _view.ListSickByRegionAtDateFromGui(
                atDate, atTime,
                region,
                xDays
            );
            //// zobraz osoby v tabuľke „Osoby“
            //dgPersons.Columns.Clear();
            //dgPersons.AutoGenerateColumns = true;   // krátkodobo
            ConfigurePersonsGridForPerson();
            dgPersons.ItemsSource = persons;
            tabLists.SelectedIndex = 0; // prepni na „Osoby“

            txtStatus.Text = "Zobrazený zoznam chorých osôb v kraji k dátumu.";
            LogToGui($"[SickByRegionAtDate] region='{region}', at={atDate:yyyy-MM-dd} {atTime}, X={xDays}");
        }
        private void HandleListSickAllAtDate()
        {
            var atDate = dpSickAllDate.SelectedDate; // DateTime?
            var atTime = tbSickAllTime.Text;         // string
            var xDays = tbSickAllXDays.Text;        // string

            var persons = _view.ListSickAllAtDateFromGui(atDate, atTime, xDays);

            ConfigurePersonsGridForPerson();

            dgPersons.ItemsSource = persons;
            tabLists.SelectedIndex = 0; // „Osoby“

            txtStatus.Text = "Zobrazený zoznam chorých osôb k zadanému dátumu.";
            LogToGui($"[SickAllAtDate] at={atDate:yyyy-MM-dd} {atTime}, X={xDays}, count={(persons as ICollection<Person>)?.Count}");
        }
        public record SickWithTest(Person Person, PCRTest Test);
        private void HandleListTopSickPerDistrictAtDate()
        {
            var atDate = dpTopSickPerDistrictDate.SelectedDate;
            var atTime = tbTopSickPerDistrictTime.Text;
            var xDays = tbTopSickPerDistrictXDays.Text;

            var rows = _view.ListTopSickPerDistrictAtDateFromGui(atDate, atTime, xDays);

            // Ak controller vracia ValueTuple (Person,PCRTest), zabaľ ho:
            var viewRows = rows.Select(r => new SickWithTest(
                Person: r.Item1,   // ak je to tuple
                Test: r.Item2
            )).ToList();

            ConfigurePersonsGridForSickWithTest();
            dgPersons.ItemsSource = viewRows;
            tabLists.SelectedIndex = 0;

            txtStatus.Text = "Zobrazené top choré osoby (1 z každého okresu) k zadanému dátumu.";
            LogToGui($"[TopSickPerDistrict] at={atDate:yyyy-MM-dd} {atTime}, X={xDays}, count={viewRows.Count}");
        }
        private void HandleListDistrictsBySickCountAtDate()
        {
            var atDate = dpSickDistrictsDate.SelectedDate; // DateTime?
            var atTime = tbSickDistrictsTime.Text;         // string
            var xDays = tbSickDistrictsXDays.Text;        // string

            var rows = _view.ListDistrictsBySickCountAtDateFromGui(atDate, atTime, xDays);
            dgDistricts.ItemsSource = null;
            dgDistricts.Items.Clear();
            dgDistricts.ItemsSource = rows
                .Select(t => Tuple.Create(t.District, t.SickCount))
                .ToList();
            // Tab index uprav podľa poradia tabov: (0=Osoby, 1=PCR testy, 2=Kraje, 3=Okresy)
            tabLists.SelectedIndex = 3;
            txtStatus.Text = "Zobrazené okresy podľa počtu chorých osôb k dátumu.";
            LogToGui($"[DistrictsBySickCount] at={atDate:yyyy-MM-dd} {atTime}, X={xDays}");
        }
        private void HandleListAllByWorkplacePeriod()
        {
            var dateFrom = dpAllWorkFrom.SelectedDate;   // DateTime?
            var timeFrom = tbAllWorkFromTime.Text;       // string
            var dateTo = dpAllWorkTo.SelectedDate;     // DateTime?
            var timeTo = tbAllWorkToTime.Text;         // string
            var workCode = tbAllWorkPlaceCode.Text;      // string

            var tests = _view.ListAllByWorkplaceFromGui(
                dateFrom, timeFrom,
                dateTo, timeTo,
                workCode
            );

            dgPcrs.ItemsSource = tests;
            tabLists.SelectedIndex = 1; // prepni na „PCR testy“
            txtStatus.Text = "Zobrazené testy za obdobie na zadanom pracovisku.";
        }
        private void HandleListSickByDistrictAtDate()
        {
            var atDate = dpSickOkresDate.SelectedDate; // DateTime?
            var atTime = tbSickOkresTime.Text;         // string
            var district = tbSickOkresDistrict.Text;     // string
            var xDays = tbSickOkresXDays.Text;        // string

            var persons = _view.ListSickByDistrictAtDateFromGui(
                atDate, atTime,
                district,
                xDays
            );

            // zobraz osoby v tabuľke „Osoby“
            ConfigurePersonsGridForPerson();
            dgPersons.ItemsSource = persons;
            tabLists.SelectedIndex = 0; // prepni na „Osoby“

            txtStatus.Text = "Zobrazený zoznam chorých osôb v okrese k dátumu.";
        }
        private void HandleListAllPeriod()
        {
            var dateFrom = dpAllFrom.SelectedDate; // DateTime?
            var timeFrom = tbAllFromTime.Text;     // string
            var dateTo = dpAllTo.SelectedDate;   // DateTime?
            var timeTo = tbAllToTime.Text;       // string

            var tests = _view.ListAllFromGui(
                dateFrom, timeFrom,
                dateTo, timeTo
            );

            dgPcrs.ItemsSource = tests;
            tabLists.SelectedIndex = 1; // „PCR testy“
            txtStatus.Text = "Zobrazené všetky testy za zadané obdobie.";
        }
        private void HandleListPositiveAllPeriod()
        {
            var dateFrom = dpPosAllFrom.SelectedDate; // DateTime?
            var timeFrom = tbPosAllFromTime.Text;     // string
            var dateTo = dpPosAllTo.SelectedDate;   // DateTime?
            var timeTo = tbPosAllToTime.Text;       // string

            var tests = _view.ListPositiveAllFromGui(
                dateFrom, timeFrom,
                dateTo, timeTo
            );

            dgPcrs.ItemsSource = tests;
            tabLists.SelectedIndex = 1; // prepni na „PCR testy“
            txtStatus.Text = "Zobrazené pozitívne testy za obdobie.";
        }
        private void HandleListAllByRegionPeriod()
        {
            var dateFrom = dpAllKrajFrom.SelectedDate;  // DateTime?
            var timeFrom = tbAllKrajFromTime.Text;      // string
            var dateTo = dpAllKrajTo.SelectedDate;    // DateTime?
            var timeTo = tbAllKrajToTime.Text;        // string
            var region = tbAllKrajRegion.Text;        // string

            var tests = _view.ListAllByRegionFromGui(
                dateFrom, timeFrom,
                dateTo, timeTo,
                region
            );

            dgPcrs.ItemsSource = tests;
            tabLists.SelectedIndex = 1; // „PCR testy“
            txtStatus.Text = "Zobrazené testy za obdobie pre kraj.";
        }
        private void HandleListPositiveByRegionPeriod()
        {
            var dateFrom = dpPosKrajFrom.SelectedDate;   // DateTime?
            var timeFrom = tbPosKrajFromTime.Text;       // string
            var dateTo = dpPosKrajTo.SelectedDate;     // DateTime?
            var timeTo = tbPosKrajToTime.Text;         // string
            var region = tbPosKrajRegion.Text;         // string

            var tests = _view.ListPositiveByRegionFromGui(
                dateFrom, timeFrom,
                dateTo, timeTo,
                region
            );

            dgPcrs.ItemsSource = tests;
            tabLists.SelectedIndex = 1; // „PCR testy“
            txtStatus.Text = "Zobrazené pozitívne testy za obdobie pre kraj.";
        }
        private void HandleListAllByDistrictPeriod()
        {
            var dateFrom = dpAllOkresFrom.SelectedDate;   // DateTime?
            var timeFrom = tbAllOkresFromTime.Text;       // string
            var dateTo = dpAllOkresTo.SelectedDate;     // DateTime?
            var timeTo = tbAllOkresToTime.Text;         // string
            var district = tbAllOkresDistrict.Text;       // string

            var tests = _view.ListAllByDistrictFromGui(
                dateFrom, timeFrom,
                dateTo, timeTo,
                district
            );

            dgPcrs.ItemsSource = tests;
            tabLists.SelectedIndex = 1; // prepni na „PCR testy“
            txtStatus.Text = "Zobrazené testy za zadané obdobie pre okres.";
        }
        private void HandleListPositiveByDistrictPeriod() 
        {
            // nič neparsujem, nič nevalidujem – posúvam „surové“ vstupy do Controller-a
            var dateFrom = dpPosOkresFrom.SelectedDate;     // DateTime?
            var timeFrom = tbPosOkresFromTime.Text;         // string
            var dateTo = dpPosOkresTo.SelectedDate;       // DateTime?
            var timeTo = tbPosOkresToTime.Text;           // string
            var district = tbPosOkresDistrict.Text;         // string

            var tests = _view.ListPositiveByDistrictFromGui(
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

            var tests = _view.ListPcrForPersonFromGui(personIdText);

            dgPcrs.ItemsSource = tests;   // zobraz do tabuľky PCR testy
            tabLists.SelectedIndex = 1;   // prepni na „PCR testy“

            txtStatus.Text = "Zobrazené PCR testy pre pacienta.";
            LogToGui($"[ListPCR-ByPerson] personId='{personIdText}' → loaded.");
        }
        private void HandleFindPcrForPerson()
        {
            var (person, pcr) = _view.FindPcrForPersonFromGui(
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
            ConfigurePersonsGridForPerson();
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
            _view.InsertPcrFromGui(
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

            _view.SeedSystemFromGui(
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
        private void ConfigurePersonsGridForPerson()
        {
            dgPersons.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            dgPersons.AutoGenerateColumns = false;
            dgPersons.Columns.Clear();
            dgPersons.Columns.Add(new DataGridTextColumn { Header = "ID pacienta", Binding = new Binding("UniqueNumber"), Width = 140 });
            dgPersons.Columns.Add(new DataGridTextColumn { Header = "Meno", Binding = new Binding("FirstName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgPersons.Columns.Add(new DataGridTextColumn { Header = "Priezvisko", Binding = new Binding("LastName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgPersons.Columns.Add(new DataGridTextColumn { Header = "Dátum nar.", Binding = new Binding("Birth") { StringFormat = "yyyy-MM-dd" }, Width = 140 });
        }

        private void ConfigurePersonsGridForSickWithTest()
        {
            dgPersons.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            dgPersons.AutoGenerateColumns = false;
            dgPersons.Columns.Clear();
            dgPersons.Columns.Add(new DataGridTextColumn { Header = "ID pacienta", Binding = new Binding("Person.UniqueNumber"), Width = 140 });
            dgPersons.Columns.Add(new DataGridTextColumn { Header = "Meno", Binding = new Binding("Person.FirstName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgPersons.Columns.Add(new DataGridTextColumn { Header = "Priezvisko", Binding = new Binding("Person.LastName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgPersons.Columns.Add(new DataGridTextColumn { Header = "Dátum nar.", Binding = new Binding("Person.Birth") { StringFormat = "yyyy-MM-dd" }, Width = 140 });
        }
        public sealed class RegionSickRow
        {
            public int RegionId { get; init; }
            public int SickCount { get; init; }
        }
    }
}