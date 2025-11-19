using PrviProjekat.Models;
using PrviProjekat.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PrviProjekat.Views
{
    public partial class NarudzbeKupacWindow : Window, INotifyPropertyChanged
    {
        private string _status = "U obradi";
        private int? _daysBack = 7;
        private string _searchKupac = "";
        private readonly ObservableCollection<object> _narudzbePrikaz = new();

        public ObservableCollection<object> NarudzbePrikaz => _narudzbePrikaz;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public NarudzbeKupacWindow() // ← SAMO OVAJ KONSTRUKTOR
        {
            InitializeComponent();
            DataContext = this;
            UcitajNarudzbe();
            OrdersGrid.MouseDoubleClick += OrdersGrid_MouseDoubleClick;
        }
        private void UcitajNarudzbe()
        {
            var sveNarudzbe = NarudzbaService.GetByStatusAndPeriod(_status, _daysBack) ?? new List<Narudzba>();

            var filtrirane = string.IsNullOrWhiteSpace(SearchKupac)
                ? sveNarudzbe
                : sveNarudzbe.Where(n =>
                      (n.ImeKupca?.Contains(SearchKupac, StringComparison.OrdinalIgnoreCase) == true) ||
                      (n.Napomena?.Contains(SearchKupac, StringComparison.OrdinalIgnoreCase) == true))
                  .ToList();

            _narudzbePrikaz.Clear();
            foreach (var n in filtrirane)
            {
                _narudzbePrikaz.Add(new
                {
                    Kupac = n.ImeKupca ?? "Nepoznat kupac",
                    Datum = n.Datum.ToString("dd.MM.yyyy HH:mm"),
                    Status = n.Status,
                    Ukupno = $"{n.Ukupno:F2} KM",
                    Stavke = string.Join(", ", n.Stavke.Select(s => $"{s.NazivProizvoda} x{s.Kolicina}")),
                    Napomena = n.Napomena ?? ""
                });
            }
        }
        private void Obradi_Click(object sender, RoutedEventArgs e)
        {
            _status = "U obradi";
            UcitajNarudzbe();

            // U obradi → tamna debela ivica
            ((Border)BtnPending.Template.FindName("border", BtnPending)).BorderThickness = new Thickness(3);
            ((Border)BtnPending.Template.FindName("border", BtnPending)).BorderBrush = new SolidColorBrush(Color.FromRgb(44, 62, 80)); // #2C3E50

            // Ostala dva vraćamo na "neaktivno"
            ((Border)BtnCompleted.Template.FindName("border", BtnCompleted)).BorderThickness = new Thickness(2);
            ((Border)BtnCompleted.Template.FindName("border", BtnCompleted)).BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));

            ((Border)BtnCanceled.Template.FindName("border", BtnCanceled)).BorderThickness = new Thickness(2);
            ((Border)BtnCanceled.Template.FindName("border", BtnCanceled)).BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
        }

        private void Zavrsene_Click(object sender, RoutedEventArgs e)
        {
            _status = "Završeno";
            UcitajNarudzbe();

            ((Border)BtnCompleted.Template.FindName("border", BtnCompleted)).BorderThickness = new Thickness(3);
            ((Border)BtnCompleted.Template.FindName("border", BtnCompleted)).BorderBrush = new SolidColorBrush(Color.FromRgb(44, 62, 80));

            ((Border)BtnPending.Template.FindName("border", BtnPending)).BorderThickness = new Thickness(2);
            ((Border)BtnPending.Template.FindName("border", BtnPending)).BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12"));

            ((Border)BtnCanceled.Template.FindName("border", BtnCanceled)).BorderThickness = new Thickness(2);
            ((Border)BtnCanceled.Template.FindName("border", BtnCanceled)).BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
        }

        private void Otkazane_Click(object sender, RoutedEventArgs e)
        {
            _status = "Otkazano";
            UcitajNarudzbe();

            ((Border)BtnCanceled.Template.FindName("border", BtnCanceled)).BorderThickness = new Thickness(3);
            ((Border)BtnCanceled.Template.FindName("border", BtnCanceled)).BorderBrush = new SolidColorBrush(Color.FromRgb(44, 62, 80));

            ((Border)BtnPending.Template.FindName("border", BtnPending)).BorderThickness = new Thickness(2);
            ((Border)BtnPending.Template.FindName("border", BtnPending)).BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12"));

            ((Border)BtnCompleted.Template.FindName("border", BtnCompleted)).BorderThickness = new Thickness(2);
            ((Border)BtnCompleted.Template.FindName("border", BtnCompleted)).BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
        }
        private void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PeriodComboBox.SelectedItem is not ComboBoxItem item) return;

            string tag = item.Tag?.ToString();

            _daysBack = tag switch
            {
                "7" => 7,
                "14" => 14,
                "30" => 30,
                "180" => 180,
                "365" => 365,
                "all" => null,
                _ => 7
            };

            UcitajNarudzbe();
        }

        
        private void OrdersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrdersGrid.SelectedItem is var selected && selected != null)
            {
                var kupacProp = selected.GetType().GetProperty("Kupac");
                var datumProp = selected.GetType().GetProperty("Datum");
                if (kupacProp == null || datumProp == null) return;

                var kupac = kupacProp.GetValue(selected)?.ToString();
                var datumStr = datumProp.GetValue(selected)?.ToString();

                var sveNarudzbe = NarudzbaService.GetByStatusAndPeriod(_status, _daysBack) ?? new List<Narudzba>();
                var narudzba = sveNarudzbe.FirstOrDefault(n =>
                    n.ImeKupca == kupac &&
                    n.Datum.ToString("dd.MM.yyyy HH:mm") == datumStr);

                if (narudzba != null)
                {
                    var detalji = new NarudzbaDetaljiWindow(narudzba);
                    detalji.Owner = this;
                    detalji.ShowDialog();

                    UcitajNarudzbe(); // ← OSVJEŽI LISTU
                }
            }
        }

      
        public string SearchKupac
        {
            get => _searchKupac;
            set
            {
                if (_searchKupac != value)
                {
                    _searchKupac = value ?? "";
                    OnPropertyChanged();
                    UcitajNarudzbe(); // automatski filtrira dok kucaš
                }
            }
        }



    }
}