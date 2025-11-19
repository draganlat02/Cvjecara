using Microsoft.VisualBasic; // za InputBox
using PrviProjekat.Models;
using PrviProjekat.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PrviProjekat.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _trenutniStatus = "U obradi";
        private ObservableCollection<object> _narudzbePrikaz = new ObservableCollection<object>();

        public ObservableCollection<object> NarudzbePrikaz
        {
            get => _narudzbePrikaz;
            set
            {
                _narudzbePrikaz = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Učitaj temu iz baze
            if (App.TrenutniKorisnik != null)
            {
                string tema = KorisnikService.GetTema(App.TrenutniKorisnik.Id);
                UpdateThemeRadioButtons(tema); // Postavi krugove
            }


        }
        private string GetString(string key, params object[] args)
        {
            var format = (string)FindResource(key);
            return args.Length > 0 ? string.Format(format, args) : format;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OrdersPanel.Visibility = Visibility.Visible;
            ProductsPanel.Visibility = Visibility.Collapsed;
            UcitajNarudzbe();

            SetStatusWithBorder("U obradi");
        }

        // === DUGMAD ===
        private void PregledNarudzbi_Click(object sender, RoutedEventArgs e)
        {
            OrdersPanel.Visibility = Visibility.Visible;
            ProductsPanel.Visibility = Visibility.Collapsed;
            UcitajNarudzbe();
        }

        private void Proizvodi_Click(object sender, RoutedEventArgs e)
        {
            OrdersPanel.Visibility = Visibility.Collapsed;
            ProductsPanel.Visibility = Visibility.Visible;
            LoadProducts();
        }

        private void DodajProizvod_Click(object sender, RoutedEventArgs e)
        {
            App.ApplyTheme(KorisnikService.GetTema(App.TrenutniKorisnik.Id));
            var win = new AddProizvodWindow();
            win.Owner = this;
            win.ShowDialog();
            LoadProducts(); // osvježi listu
        }

        private void UrediProizvod_Click(object sender, RoutedEventArgs e)
        {
            var urediWindow = new UrediProizvodeWindow();
            urediWindow.Owner = this;
            urediWindow.ShowDialog();
            LoadProducts();
        }

        private void Zaposleni_Click(object sender, RoutedEventArgs e)
        {
            var upravljanjeWindow = new UpravljanjeZaposlenimaWindow();
            upravljanjeWindow.Owner = this;
            upravljanjeWindow.ShowDialog();
        }

        private void Odjava_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        // === STATUS DUGMAD ===
        private void Obradi_Click(object sender, RoutedEventArgs e) => SetStatusWithBorder("U obradi");
        private void Zavrsene_Click(object sender, RoutedEventArgs e) => SetStatusWithBorder("Završeno");
        private void Otkazane_Click(object sender, RoutedEventArgs e) => SetStatusWithBorder("Otkazano");

        private void SetStatusWithBorder(string status)
        {
            _trenutniStatus = status;
            UcitajNarudzbe();

            // Resetuj sve na "neaktivno"
            ResetAllBorders();

            // Aktiviraj samo trenutno
            var activeBorder = status switch
            {
                "U obradi" => (Border)BtnPendingMain.Template.FindName("border", BtnPendingMain),
                "Završeno" => (Border)BtnCompletedMain.Template.FindName("border", BtnCompletedMain),
                "Otkazano" => (Border)BtnCanceledMain.Template.FindName("border", BtnCanceledMain),
                _ => null
            };

            if (activeBorder != null)
            {
                activeBorder.BorderThickness = new Thickness(3);
                activeBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(44, 62, 80)); // #2C3E50 tamna ivica
            }
        }

        private void ResetAllBorders()
        {
            // U obradi
            var b1 = (Border)BtnPendingMain.Template.FindName("border", BtnPendingMain);
            b1.BorderThickness = new Thickness(2);
            b1.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12"));

            // Završeno
            var b2 = (Border)BtnCompletedMain.Template.FindName("border", BtnCompletedMain);
            b2.BorderThickness = new Thickness(2);
            b2.BorderBrush = (SolidColorBrush)FindResource("SuccessBrush");

            // Otkazano
            var b3 = (Border)BtnCanceledMain.Template.FindName("border", BtnCanceledMain);
            b3.BorderThickness = new Thickness(2);
            b3.BorderBrush = (SolidColorBrush)FindResource("DangerBrush");
        }

        private void SetStatus(string status)
        {
            _trenutniStatus = status;
            UcitajNarudzbe();
        }

        private void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UcitajNarudzbe();
        }

        // === UČITAVANJE NARUDŽBI ===
        private void UcitajNarudzbe()
        {
            if (OrdersGrid == null || PeriodComboBox == null) return;

            int? daysBack = GetDaysBackFromPeriod();
            var narudzbe = NarudzbaService.GetByStatusAndPeriod(_trenutniStatus, daysBack) ?? new List<Narudzba>();

            NarudzbePrikaz.Clear();
            foreach (var n in narudzbe)
            {
                NarudzbePrikaz.Add(new
                {
                    n.Id,
                    ImeKupca = n.ImeKupca,
                    Datum = n.Datum,
                    Napomena = n.Napomena,
                    Status = n.Status,
                    Ukupno = n.Ukupno,
                    Stavke = string.Join(", ", n.Stavke.Select(s => $"{s.NazivProizvoda} x{s.Kolicina}"))
                });
            }

            // Zarada
            if (_trenutniStatus == "Završeno")
            {
                decimal zarada = narudzbe.Sum(n => n.Ukupno);
                ZaradaText.Text = GetString("Total_Earnings", zarada);
            }
            else
            {
                ZaradaText.Text = "";
            }
        }

        private int? GetDaysBackFromPeriod()
        {
            if (PeriodComboBox.SelectedItem is not ComboBoxItem item) return 7;

            string tag = item.Tag?.ToString();

            return tag switch
            {
                "7" => 7,
                "14" => 14,
                "30" => 30,
                "180" => 180,
                "365" => 365,
                "all" => null,
                _ => 7
            };
        }

        // === PROIZVODI ===
        private void LoadProducts()
        {
            var proizvodi = ProizvodService.GetAll() ?? new List<Proizvod>();
            ProductsList.ItemsSource = proizvodi;
        }

        // === PROMJENA STATUSA ===
        // === PROMJENA STATUSA (SADA KORISTI PromjenaStatusaWindow) ===
        private void PromijeniStatus_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem == null)
            {
                MessageBox.Show(GetString("Msg_SelectOrder"), "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = OrdersGrid.SelectedItem;
            var idProp = selected.GetType().GetProperty("Id");
            if (idProp == null) return;

            int narudzbaId = (int)idProp.GetValue(selected);
            string trenutniStatus = selected.GetType().GetProperty("Status")?.GetValue(selected)?.ToString() ?? "";

            var prozor = new PromjenaStatusaWindow(trenutniStatus);
            if (prozor.ShowDialog() == true && !string.IsNullOrEmpty(prozor.NoviStatus))
            {
                int noviStatusId = GetStatusIdByName(prozor.NoviStatus);
                try
                {
                    NarudzbaService.UpdateStatus(narudzbaId, noviStatusId);
                    MessageBox.Show(GetString("OrderDetails_StatusChanged", prozor.NoviStatus), "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
                    UcitajNarudzbe();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(GetString("OrderDetails_ErrorUpdate", ex.Message), "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // === DESNI KLIK ===
        private void OrdersGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement(OrdersGrid, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row == null) return;

            OrdersGrid.SelectedItem = row.Item;
            var selected = OrdersGrid.SelectedItem;
            var idProp = selected.GetType().GetProperty("Id");
            if (idProp == null) return;
            int narudzbaId = (int)idProp.GetValue(selected);
            string trenutniStatus = selected.GetType().GetProperty("Status")?.GetValue(selected)?.ToString() ?? "";

            var prozor = new PromjenaStatusaWindow(trenutniStatus);
            if (prozor.ShowDialog() == true && !string.IsNullOrEmpty(prozor.NoviStatus))
            {
                int noviStatusId = GetStatusIdByName(prozor.NoviStatus);
                try
                {
                    NarudzbaService.UpdateStatus(narudzbaId, noviStatusId);
                    MessageBox.Show($"Status #{narudzbaId} → '{prozor.NoviStatus}'", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
                    UcitajNarudzbe();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Greška: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private int GetStatusIdByName(string naziv)
        {
            return naziv switch
            {
                "U obradi" => 1,
                "Otkazano" => 4,
                "Završeno" => 7,
                _ => 1
            };
        }

        // === DVOKLIK ===
        private void OrdersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrdersGrid.SelectedItem == null) return;

            var selected = OrdersGrid.SelectedItem;
            var idProp = selected.GetType().GetProperty("Id");
            if (idProp == null) return;
            int narudzbaId = (int)idProp.GetValue(selected);

            var sveNarudzbe = NarudzbaService.GetByStatusAndPeriod(_trenutniStatus, null) ?? new List<Narudzba>();
            var pravaNarudzba = sveNarudzbe.FirstOrDefault(n => n.Id == narudzbaId);
            if (pravaNarudzba == null) return;

            var detaljiWindow = new NarudzbaDetaljiWindow(pravaNarudzba);
            detaljiWindow.Owner = this;
            if (detaljiWindow.ShowDialog() == true)
            {
                UcitajNarudzbe();
            }
        }

        // === TEMA ===


        private void ThemeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton rb || rb.Tag is not string tema) return;

            // 1. SAČUVAJ TRENUTNI JEZIK
            var currentLang = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("/Languages/") == true);

            // 2. UKLONI SAMO STARU TEMU
            var oldTheme = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("/Themes/") == true);
            if (oldTheme != null)
                Application.Current.Resources.MergedDictionaries.Remove(oldTheme);

            // 3. DODAJ NOVU TEMU
            var uri = new Uri($"pack://application:,,,/Themes/{tema}Theme.xaml");
            var dict = new ResourceDictionary { Source = uri };
            Application.Current.Resources.MergedDictionaries.Add(dict);

            // 4. VRATI JEZIK (ako je bio)
            if (currentLang != null)
                Application.Current.Resources.MergedDictionaries.Add(currentLang);

            // 5. SAČUVAJ U BAZU
            if (App.TrenutniKorisnik != null)
            {
                KorisnikService.UpdateTema(App.TrenutniKorisnik.Id, tema);
                App.TrenutniKorisnik.Tema = tema; // ažuriraj memoriju
            }

            // 6. OSVJEŽI RADIO DUGMAD
            UpdateThemeRadioButtons(tema);
        }

        // Poziva se iz konstruktora i prilikom promjene
        private void UpdateThemeRadioButtons(string currentTheme)
        {
            var windows = Application.Current.Windows.OfType<Window>();
            foreach (var win in windows)
            {
                if (win is MainWindow mw)
                {
                    mw.LightRadioButton.IsChecked = currentTheme == "Light";
                    mw.PinkRadioButton.IsChecked = currentTheme == "Pink";
                    mw.OceanRadioButton.IsChecked = currentTheme == "Ocean";
                }
                else if (win is KupacPanel kp)
                {
                    kp.LightRadioButton.IsChecked = currentTheme == "Light";
                    kp.PinkRadioButton.IsChecked = currentTheme == "Pink";
                    kp.OceanRadioButton.IsChecked = currentTheme == "Ocean";
                }
            }
        }

        private void SetRadioButtonChecked(object element, bool isChecked)
        {
            if (element is RadioButton rb)
                rb.IsChecked = isChecked;
        }
        private void Lang_BS_Click(object sender, RoutedEventArgs e)
        {
            App.ChangeLanguage("bs-BA");

            // AŽURIRAJ U BAZI
            KorisnikService.UpdateJezik(App.TrenutniKorisnik.Id, "bs-BA");

            // OPCIJA: AŽURIRAJ I U MEMORIJI (ako koristiš)
            App.TrenutniKorisnik.Jezik = "bs-BA";
        }

        private void Lang_EN_Click(object sender, RoutedEventArgs e)
        {
            App.ChangeLanguage("en-US");

            // AŽURIRAJ U BAZI
            KorisnikService.UpdateJezik(App.TrenutniKorisnik.Id, "en-US");

            // OPCIJA: AŽURIRAJ I U MEMORIJI
            App.TrenutniKorisnik.Jezik = "en-US";
        }



    }
}