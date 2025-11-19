using PrviProjekat.Models;
using PrviProjekat.Services;
using System;
using System.Windows;

namespace PrviProjekat.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Učitaj default temu ako nema korisnika (npr. prvi put)
            if (App.TrenutniKorisnik == null)
            {
                App.ApplyTheme("Light"); // ili "Pink" ako želiš
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Molimo unesite korisničko ime i lozinku.", "Greška",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var korisnik = KorisnikService.Login(username, password);
            if (korisnik != null)
            {
                App.TrenutniKorisnik = korisnik;

                // TEMA
                string tema = KorisnikService.GetTema(korisnik.Id) ?? "Light";
                App.ApplyTheme(tema);

                // JEZIK – DODAJ OVO!
                string jezik = KorisnikService.GetJezik(korisnik.Id) ?? "bs-BA";
                App.ChangeLanguage(jezik);
                App.TrenutniKorisnik.Jezik = jezik;// OVO MIJENJA SVE RIJEČI

                // Otvori panel
                Window panel = korisnik.Tip == TipKorisnika.Administrator
                    ? new MainWindow()
                    : new KupacPanel();
                panel.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Pogrešno korisničko ime ili lozinka.", "Greška",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}