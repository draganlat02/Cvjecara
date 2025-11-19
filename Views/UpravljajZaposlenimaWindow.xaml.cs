using PrviProjekat.Models;
using PrviProjekat.Services;
using System.Windows;

namespace PrviProjekat.Views
{
    public partial class UpravljanjeZaposlenimaWindow : Window
    {
        private Korisnik selectedKorisnik = null;

        public UpravljanjeZaposlenimaWindow()
        {
            InitializeComponent();
            LoadZaposleni();
        }

        private void LoadZaposleni()
        {
            var zaposleni = KorisnikService.GetAll();
            ZaposleniGrid.ItemsSource = zaposleni;

            // Ako nema zaposlenih → prikaži poruku
            if (zaposleni == null || zaposleni.Count == 0)
            {
               
                ZaposleniGrid.ItemsSource = new[]
                {
            new { Ime = "Nema zaposlenih za prikaz", Prezime = "", Kontakt = "", Username = "", Tip = "" }
        };
                ZaposleniGrid.IsEnabled = false; // onemogući selekciju
            }
            else
            {
                ZaposleniGrid.IsEnabled = true;
            }
        }

        private void ClearForm()
        {
            selectedKorisnik = null;
            ImeTextBox.Clear();
            PrezimeTextBox.Clear();
            KontaktTextBox.Clear();
            UsernameTextBox.Clear();
            PasswordTextBox.Clear();
            PozicijaComboBox.SelectedIndex = -1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string ime = ImeTextBox.Text.Trim();
            string prezime = PrezimeTextBox.Text.Trim();
            string kontakt = KontaktTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordTextBox.Text.Trim();

            // 1. Obavezna polja
            if (string.IsNullOrWhiteSpace(ime) ||
                string.IsNullOrWhiteSpace(prezime) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                PozicijaComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Sva polja su obavezna osim kontakta!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Ime i prezime – samo slova i razmak
            if (!IsOnlyLetters(ime))
            {
                MessageBox.Show("Ime može sadržavati samo slova i razmake!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsOnlyLetters(prezime))
            {
                MessageBox.Show("Prezime može sadržavati samo slova i razmake!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Kontakt – ako je unesen, mora biti samo brojevi i minimalno 9 cifara (npr. 062123456)
            if (!string.IsNullOrWhiteSpace(kontakt))
            {
                if (!IsOnlyNumbers(kontakt))
                {
                    MessageBox.Show("Kontakt može sadržavati samo brojeve!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (kontakt.Length < 9 || kontakt.Length > 10)
                {
                    MessageBox.Show("Kontakt mora imati 9 ili 10 brojeva (npr. 062123456 ili 0611234567)!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!kontakt.StartsWith("06"))
                {
                    MessageBox.Show("Kontakt mora počinjati sa 06 (npr. 062, 063, 064...)", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // 4. Username – minimalno 4 karaktera, samo slova, brojevi i donja crta
            if (username.Length < 4)
            {
                MessageBox.Show("Username mora imati najmanje 4 karaktera!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                MessageBox.Show("Username može sadržavati samo slova, brojeve i donju crtu (_)", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 5. Password – minimalno 6 karaktera
            if (password.Length < 6)
            {
                MessageBox.Show("Lozinka mora imati najmanje 6 karaktera!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 6. Provjera da li username već postoji (osim za trenutnog korisnika kod editovanja)
            if (selectedKorisnik == null)
            {
                if (KorisnikService.UsernameExists(username))
                {
                    MessageBox.Show("Ovaj username već postoji! Izaberi drugi.", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                if (KorisnikService.UsernameExists(username, selectedKorisnik.Id))
                {
                    MessageBox.Show("Ovaj username već koristi drugi zaposleni!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // SVE JE OK – SPAŠAVAMO
            if (selectedKorisnik == null)
            {
                // NOVI ZAPOSLENI
                var novi = new Korisnik
                {
                    Ime = ime,
                    Prezime = prezime,
                    Kontakt = kontakt, // može biti prazan string
                    Username = username,
                    Password = password, // kasnije možeš hash-ovati
                    Tip = PozicijaComboBox.SelectedIndex == 0 ? TipKorisnika.Administrator : TipKorisnika.Referent
                };

                KorisnikService.AddKorisnik(novi);
                MessageBox.Show($"Zaposleni {ime} {prezime} uspješno dodan!", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // UREĐIVANJE
                selectedKorisnik.Ime = ime;
                selectedKorisnik.Prezime = prezime;
                selectedKorisnik.Kontakt = kontakt;
                selectedKorisnik.Username = username;
                selectedKorisnik.Password = password;
                selectedKorisnik.Tip = PozicijaComboBox.SelectedIndex == 0 ? TipKorisnika.Administrator : TipKorisnika.Referent;

                KorisnikService.UpdateKorisnik(selectedKorisnik);
                MessageBox.Show($"Zaposleni {ime} {prezime} uspješno ažuriran!", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            ClearForm();
            LoadZaposleni();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ZaposleniGrid.SelectedItem is Korisnik k)
            {
                var result = MessageBox.Show($"Jeste li sigurni da želite obrisati {k.Ime} {k.Prezime}?",
                                            "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    KorisnikService.DeleteKorisnik(k.Id);

                    // RESETUJ SVE
                    ClearForm();
                    LoadZaposleni(); // osvježi + prikaži poruku ako je prazno
                }
            }
            else
            {
                MessageBox.Show("Odaberite zaposlenog za brisanje.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearFields_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ZaposleniGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ZaposleniGrid.SelectedItem is Korisnik k)
            {
                selectedKorisnik = k;
                ImeTextBox.Text = k.Ime;
                PrezimeTextBox.Text = k.Prezime;
                KontaktTextBox.Text = k.Kontakt;
                UsernameTextBox.Text = k.Username;
                PasswordTextBox.Text = k.Password;
                PozicijaComboBox.SelectedIndex = k.Tip == TipKorisnika.Administrator ? 0 : 1;
            }
        }

        private bool IsOnlyLetters(string input)
        {
            return input.All(c => char.IsLetter(c) || c == ' ');
        }

        private bool IsOnlyNumbers(string input)
        {
            return input.All(char.IsDigit);
        }
    }
}
