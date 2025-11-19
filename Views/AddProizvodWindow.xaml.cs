using System;
using System.Linq;
using System.Windows;
using PrviProjekat.Models;
using PrviProjekat.Services;

namespace PrviProjekat.Views
{
    public partial class AddProizvodWindow : Window
    {
        public AddProizvodWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void AddProizvodWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var kategorije = KategorijaService.GetAll();
                KategorijaComboBox.ItemsSource = kategorije;

                if (kategorije.Count > 1)
                    KategorijaComboBox.SelectedIndex = 1; // automatski izaberi drugu kategoriju
                else if (kategorije.Count == 1)
                    KategorijaComboBox.SelectedIndex = 0; // ako ima samo jedna
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju kategorija: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Sacuvaj_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Provjera da su sva polja popunjena
                if (string.IsNullOrWhiteSpace(NazivTextBox.Text) ||
                    string.IsNullOrWhiteSpace(CijenaTextBox.Text) ||
                    string.IsNullOrWhiteSpace(SifraTextBox.Text) ||
                    KategorijaComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Molimo popunite sva polja!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ✔ VALIDACIJA: Naziv samo slova
                if (!NazivTextBox.Text.All(c => char.IsLetter(c) || c == ' '))
                {
                    MessageBox.Show("Naziv može sadržavati samo slova!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ✔ VALIDACIJA: Šifra samo slova i brojevi
                if (!SifraTextBox.Text.All(char.IsLetterOrDigit))
                {
                    MessageBox.Show("Šifra može sadržavati samo slova i brojeve!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // Parsing cijene (robustan, nezavisan o regionalnim postavkama)
                if (!decimal.TryParse(CijenaTextBox.Text.Trim(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal cijena))
                {
                    MessageBox.Show("Unesite ispravan format cijene!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Provjera da li proizvod sa istom šifrom već postoji
                var sviProizvodi = ProizvodService.GetAll();
                if (sviProizvodi.Any(p => p.Sifra.Equals(SifraTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Proizvod sa ovom šifrom već postoji!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Odabrana kategorija
                var odabranaKategorija = (Kategorija)KategorijaComboBox.SelectedItem;

                var proizvod = new Proizvod
                {
                    Naziv = NazivTextBox.Text.Trim(),
                    Cijena = cijena,
                    Sifra = SifraTextBox.Text.Trim(),
                    Opis = OpisTextBox.Text.Trim(),
                    Kolicina = 1,
                    Kategorija_idKATEGORIJA = odabranaKategorija.idKATEGORIJA,
                    Dostupan = true
                };

                ProizvodService.DodajProizvod(proizvod);

                MessageBox.Show("Proizvod je uspješno dodat!", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri čuvanju proizvoda: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadCategories()
        {
            try
            {
                var kategorije = KategorijaService.GetAll();
                KategorijaComboBox.ItemsSource = kategorije;
                KategorijaComboBox.DisplayMemberPath = "Naziv";
                KategorijaComboBox.SelectedValuePath = "idKATEGORIJA";

                if (kategorije.Count > 0)
                    KategorijaComboBox.SelectedIndex = 0; // automatski odabir prve kategorije
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju kategorija: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

      

    }
}
