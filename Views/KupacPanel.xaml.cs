using PrviProjekat.Models;
using PrviProjekat.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PrviProjekat.Views
{
    public partial class KupacPanel : Window
    {
        private List<Proizvod> proizvodi = new List<Proizvod>();
        private List<Kategorija> kategorije = new List<Kategorija>();
        private List<Proizvod> korpa = new List<Proizvod>();
        private ICollectionView view;


        private string _searchQuery = "";
        public KupacPanel()
        {
            InitializeComponent(); // ← PRVO!
            DataContext = this;

            if (App.TrenutniKorisnik != null)
            {
                // 1. UČITAJ TEMU IZ BAZE
                string temaIzBaze = KorisnikService.GetTema(App.TrenutniKorisnik.Id);

                // 2. AŽURIRAJ U MEMORIJI
                App.TrenutniKorisnik.Tema = temaIzBaze;

                // 3. PRIMIJENI TEMU
                App.ApplyTheme(temaIzBaze);

                // 4. PROMIJENI JEZIK
                App.ChangeLanguage(App.TrenutniKorisnik.Jezik ?? "bs-BA");

                // 5. OZNAČI RADIO DUGMAD
                UpdateThemeRadioButtons(temaIzBaze);
            }
            else
            {
                App.ChangeLanguage("bs-BA");
                App.ApplyTheme("Light");
            }

            // Ostatak inicijalizacije...
            kategorije = KategorijaService.GetAll() ?? new List<Kategorija>();
            LoadCategories();
            UcitajProizvode();
            view = CollectionViewSource.GetDefaultView(proizvodi);
            ProductsList.ItemsSource = view;
            UpdateCartCount();
            ApplyFilters();

        }

        private void LoadCategories()
        {
            if (CategoryFilterComboBox == null) return;
            CategoryFilterComboBox.Items.Clear();

            // OVO JE BILO POGREŠNO: Tag = "0" kao string
            CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = "Sve kategorije", Tag = 0 }); // int 0

            foreach (var kat in kategorije)
            {
                CategoryFilterComboBox.Items.Add(new ComboBoxItem
                {
                    Content = kat.Naziv,
                    Tag = kat.idKATEGORIJA  // ← int, ne string!
                });
            }

     (CategoryFilterComboBox.Items[0] as ComboBoxItem).IsSelected = true;
        }

        private void UcitajProizvode()
        {
            proizvodi = ProizvodService.GetAll() ?? new List<Proizvod>();
            foreach (var p in proizvodi)
            {
                var kat = kategorije.FirstOrDefault(k => k.idKATEGORIJA == p.Kategorija_idKATEGORIJA);
                p.KategorijaNaziv = kat?.Naziv ?? "N/A";
            }
            view?.Refresh();
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (view == null || SortComboBox.SelectedItem is not ComboBoxItem item) return;

            string tag = item.Tag?.ToString();
            view.SortDescriptions.Clear();

            switch (tag)
            {
                case "name_asc":
                    view.SortDescriptions.Add(new SortDescription("Naziv", ListSortDirection.Ascending));
                    break;
                case "name_desc":
                    view.SortDescriptions.Add(new SortDescription("Naziv", ListSortDirection.Descending));
                    break;
                case "price_asc":
                    view.SortDescriptions.Add(new SortDescription("Cijena", ListSortDirection.Ascending));
                    break;
                case "price_desc":
                    view.SortDescriptions.Add(new SortDescription("Cijena", ListSortDirection.Descending));
                    break;
            }

            ApplyFilters(); // Važno: osvježi i filtere nakon sortiranja
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox == null) return;

            string text = SearchTextBox.Text?.Trim() ?? "";

            // Ako je placeholder, ignoriši
            if (text == "Pretraži proizvode..." || text == "Search products...")
                text = "";

            // AŽURIRAJ SearchQuery → ovo pokreće ApplyFilters() automatski!
            SearchQuery = text;
        }
        private void OpenCart_Click(object sender, RoutedEventArgs e)
        {
            if (App.TrenutniKorisnik != null)
                App.ApplyTheme(KorisnikService.GetTema(App.TrenutniKorisnik.Id));

            var cartWindow = new KorpaWindow(korpa);
            cartWindow.Owner = this;
            cartWindow.ShowDialog();
            UpdateCartCount();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && tb.Text == tb.Tag?.ToString())
            {
                tb.Text = "";
                tb.Foreground = (Brush)FindResource("TextBrush");
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = tb.Tag?.ToString() ?? "";
                tb.Foreground = Brushes.Gray;
            }
        }

        private void PregledNarudzbi_Click(object sender, RoutedEventArgs e)
        {
            if (App.TrenutniKorisnik != null)
            {
                string tema = KorisnikService.GetTema(App.TrenutniKorisnik.Id);
                App.ApplyTheme(tema); 
            }

            var narudzbeWindow = new NarudzbeKupacWindow();
            narudzbeWindow.Owner = this;
            narudzbeWindow.ShowDialog();
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Proizvod p)
            {
                var postojeci = korpa.FirstOrDefault(x => x.Id == p.Id);
                if (postojeci != null)
                    postojeci.Kolicina++;
                else
                {
                    p.Kolicina = 1;
                    korpa.Add(p);
                }
                UpdateCartCount();
            }
        }

        private void UpdateCartCount()
        {
            int ukupno = korpa.Sum(p => p.Kolicina);  // Ako imaš Kolicina u Proizvod klasi
            CartCountText.Text = ukupno > 0 ? $" ({ukupno})" : "";
        }


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

            // 5. SAČUVAJ U BAZU – OVO JE NEDOSTAJALO!
            if (App.TrenutniKorisnik != null)
            {
                KorisnikService.UpdateTema(App.TrenutniKorisnik.Id, tema);
                App.TrenutniKorisnik.Tema = tema; // ažuriraj u memoriji
            }

            // 6. OSVJEŽI RADIO DUGMAD U SVIM PROZORIMA
            UpdateThemeRadioButtons(tema);
        }

        // Poziva se iz konstruktora i prilikom promjene
        private void UpdateThemeRadioButtons(string currentTheme)
        {
            var windows = Application.Current.Windows.OfType<Window>();
            foreach (var win in windows)
            {
                if (win is KupacPanel kp)
                {
                    kp.LightRadioButton.IsChecked = currentTheme == "Light";
                    kp.PinkRadioButton.IsChecked = currentTheme == "Pink";
                    kp.OceanRadioButton.IsChecked = currentTheme == "Ocean";
                }
                else if (win is MainWindow mw)
                {
                    mw.LightRadioButton.IsChecked = currentTheme == "Light";
                    mw.PinkRadioButton.IsChecked = currentTheme == "Pink";
                    mw.OceanRadioButton.IsChecked = currentTheme == "Ocean";
                }
            }
        }

     

        private void Lang_BS_Click(object sender, RoutedEventArgs e)
        {
            App.ChangeLanguage("bs-BA");
            KorisnikService.UpdateJezik(App.TrenutniKorisnik.Id, "bs-BA");
            App.TrenutniKorisnik.Jezik = "bs-BA";
        }

        private void Lang_EN_Click(object sender, RoutedEventArgs e)
        {
            App.ChangeLanguage("en-US");
            KorisnikService.UpdateJezik(App.TrenutniKorisnik.Id, "en-US");
            App.TrenutniKorisnik.Jezik = "en-US";
        }


        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value ?? "";
                    ApplyFilters(); // Automatski osvježava filter
                }
            }
        }

        private void ApplyFilters()
        {
            if (view == null) return;

            string query = (SearchQuery ?? "").Trim().ToLower();

            view.Filter = item =>
            {
                if (item is not Proizvod p) return false;

                // 1. Pretraga
                bool matchesSearch = string.IsNullOrEmpty(query) ||
                    p.Naziv?.ToLower().Contains(query) == true ||
                    p.Opis?.ToLower().Contains(query) == true;

                // 2. KATEGORIJA – ISPRAVLJENO!
                bool matchesCategory = true;
                if (CategoryFilterComboBox.SelectedItem is ComboBoxItem catItem)
                {
                    // Ako je Tag int i veći od 0 → filtriraj
                    if (catItem.Tag is int catId && catId > 0)
                    {
                        matchesCategory = p.Kategorija_idKATEGORIJA == catId;
                    }
                    // Ako je 0 ili null → sve kategorije
                }

                return matchesSearch && matchesCategory;
            };

            ApplySorting();
            view.Refresh();

            // PRIKAŽI / SAKRIJ PANEL ZA PRAZNE REZULTATE
            bool hasResults = view.Cast<object>().Any();

            ProductsList.Visibility = hasResults ? Visibility.Visible : Visibility.Collapsed;
            EmptyResultsPanel.Visibility = hasResults ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ApplySorting()
        {
            if (view == null || SortComboBox.SelectedItem is not ComboBoxItem item) return;

            string tag = item.Tag?.ToString();
            view.SortDescriptions.Clear();

            switch (tag)
            {
                case "name_asc":
                    view.SortDescriptions.Add(new SortDescription("Naziv", ListSortDirection.Ascending));
                    break;
                case "name_desc":
                    view.SortDescriptions.Add(new SortDescription("Naziv", ListSortDirection.Descending));
                    break;
                case "price_asc":
                    view.SortDescriptions.Add(new SortDescription("Cijena", ListSortDirection.Ascending));
                    break;
                case "price_desc":
                    view.SortDescriptions.Add(new SortDescription("Cijena", ListSortDirection.Descending));
                    break;
            }
        }
    }
}