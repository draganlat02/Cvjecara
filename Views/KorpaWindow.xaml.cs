using MySql.Data.MySqlClient;
using PrviProjekat.Database;
using PrviProjekat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PrviProjekat.Views
{
    public partial class KorpaWindow : Window
    {
        private List<Proizvod> _korpa;
        private int _zaposleniId;

        public KorpaWindow(List<Proizvod> korpa, int zaposleniId = 1)
        {
            InitializeComponent();
            _korpa = korpa;
            _zaposleniId = zaposleniId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCart();
            IzracunajUkupno();

            // JEDNOSTAVNO: Ako je prazno → prikaži panel, sakrij listu
            bool isEmpty = _korpa.Count == 0;
            EmptyCartPanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
            CartList.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoadCart()
        {
            CartList.ItemsSource = null;
            CartList.ItemsSource = _korpa;
        }

        private void IzracunajUkupno()
        {
            foreach (var p in _korpa)
                p.Ukupno = p.Cijena * p.Kolicina;
            var total = _korpa.Sum(p => p.Ukupno);
            TotalText.Text = $"Ukupno: {total:0.00} KM";
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Proizvod proizvod)
            {
                proizvod.Kolicina++;
                IzracunajUkupno();
                LoadCart();
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Proizvod p)
            {
                _korpa.Remove(p);
                LoadCart();
                IzracunajUkupno();

                // Ako postane prazno → prikaži poruku
                if (_korpa.Count == 0)
                {
                    EmptyCartPanel.Visibility = Visibility.Visible;
                    CartList.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Proizvod p)
            {
                if (p.Kolicina > 1)
                    p.Kolicina--;
                else
                    _korpa.Remove(p);

                LoadCart();
                IzracunajUkupno();

                if (_korpa.Count == 0)
                {
                    EmptyCartPanel.Visibility = Visibility.Visible;
                    CartList.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            string imeKupca = ImeKupcaTextBox.Text.Trim();
            string napomena = NapomenaTextBox.Text.Trim();

            if (string.IsNullOrEmpty(imeKupca))
            {
                MessageBox.Show("Molimo unesite ime kupca prije slanja narudžbe!",
                                "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_korpa == null || _korpa.Count == 0)
            {
                MessageBox.Show("Korpa je prazna!", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = MySqlDb.GetConnection())
                {
                    // 1. Kreiraj račun
                    string racunQuery = "INSERT INTO RACUN (Iznos, StatusRacuna_idSTATUS_RACUNA) VALUES (@iznos, 1)";
                    long racunId;
                    using (var cmdRacun = new MySqlCommand(racunQuery, conn))
                    {
                        cmdRacun.Parameters.AddWithValue("@iznos", _korpa.Sum(p => p.Cijena));
                        cmdRacun.ExecuteNonQuery();
                        racunId = cmdRacun.LastInsertedId;
                    }

                    // 2. Ubaci narudžbu
                    string narudzbaQuery = @"
                        INSERT INTO NARUDZBA
                        (Zaposleni_idZAPOSLENI, ImeKupca, Napomena, DatumNarudzbe, StatusNarudzbe_idSTATUS, Racun_idRACUN)
                        VALUES (@zaposleniId, @imeKupca, @napomena, NOW(), 1, @racunId)";
                    long narudzbaId;
                    using (var cmdNarudzba = new MySqlCommand(narudzbaQuery, conn))
                    {
                        cmdNarudzba.Parameters.AddWithValue("@zaposleniId", _zaposleniId);
                        cmdNarudzba.Parameters.AddWithValue("@imeKupca", imeKupca);
                        cmdNarudzba.Parameters.AddWithValue("@napomena", string.IsNullOrEmpty(napomena) ? DBNull.Value : napomena);
                        cmdNarudzba.Parameters.AddWithValue("@racunId", racunId);
                        cmdNarudzba.ExecuteNonQuery();
                        narudzbaId = cmdNarudzba.LastInsertedId;
                    }

                    // 3. Ubaci stavke
                    foreach (var p in _korpa)
                    {
                        string detaljiQuery = @"
                            INSERT INTO NARUDZBA_has_PROIZVOD
                            (NARUDZBA_idNARUDZBA, PROIZVOD_idPROIZVOD, Kolicina, Cijena)
                            VALUES (@narudzbaId, @proizvodId, @kolicina, @cijena)";
                        using (var cmdDetalji = new MySqlCommand(detaljiQuery, conn))
                        {
                            cmdDetalji.Parameters.AddWithValue("@narudzbaId", narudzbaId);
                            cmdDetalji.Parameters.AddWithValue("@proizvodId", p.Id);
                            cmdDetalji.Parameters.AddWithValue("@kolicina", p.Kolicina);
                            cmdDetalji.Parameters.AddWithValue("@cijena", p.Cijena);
                            cmdDetalji.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Narudžba uspješno sačuvana!", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
                _korpa.Clear();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri čuvanju narudžbe: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}