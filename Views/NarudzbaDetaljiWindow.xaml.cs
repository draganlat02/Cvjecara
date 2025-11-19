using PrviProjekat.Models;
using PrviProjekat.Services;
using System;
using System.Windows;

namespace PrviProjekat.Views
{
    public partial class NarudzbaDetaljiWindow : Window
    {
        private readonly Narudzba _narudzba;

        // KONSTRUKTOR
        public NarudzbaDetaljiWindow(Narudzba narudzba)
        {
            InitializeComponent();
            _narudzba = narudzba ?? throw new ArgumentNullException(nameof(narudzba));
            _narudzba.Napomena ??= ""; // Osiguraj da nije null
            DataContext = _narudzba;
        }

        // METOD ZA LOKALIZACIJU
        private string GetString(string key, params object[] args)
        {
            var format = (string)FindResource(key);
            return args.Length > 0 ? string.Format(format, args) : format;
        }

        // DUGME ZA PROMJENU STATUSA (PREKO PROZORA)
        private void PromijeniStatus_Click(object sender, RoutedEventArgs e)
        {
            var prozor = new PromjenaStatusaWindow(_narudzba.Status);
            if (prozor.ShowDialog() == true && !string.IsNullOrEmpty(prozor.NoviStatus))
            {
                int noviStatusId = prozor.NoviStatus switch
                {
                    "U obradi" => 1,
                    "Završeno" => 7,
                    "Otkazano" => 4,
                    _ => 1
                };

                try
                {
                    NarudzbaService.UpdateStatus(_narudzba.Id, noviStatusId);
                    MessageBox.Show(GetString("OrderDetails_StatusChanged", prozor.NoviStatus), "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(GetString("OrderDetails_ErrorUpdate", ex.Message), "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // DIREKTNE PROMJENE STATUSA
        private void SetStatusUObradi_Click(object sender, RoutedEventArgs e)
            => PromijeniStatus(1, "U obradi");

        private void SetStatusZavrseno_Click(object sender, RoutedEventArgs e)
            => PromijeniStatus(7, "Završeno");

        private void SetStatusOtkazano_Click(object sender, RoutedEventArgs e)
            => PromijeniStatus(4, "Otkazano");

        // GLAVNA METODA ZA PROMJENU STATUSA
        private void PromijeniStatus(int statusId, string opis)
        {
            try
            {
                NarudzbaService.UpdateStatus(_narudzba.Id, statusId);
                MessageBox.Show(GetString("OrderDetails_StatusUpdated", opis), "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetString("OrderDetails_ErrorUpdate", ex.Message), "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}