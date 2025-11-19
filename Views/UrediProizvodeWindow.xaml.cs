using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PrviProjekat.Models;
using PrviProjekat.Services;

namespace PrviProjekat.Views
{
    public partial class UrediProizvodeWindow : Window
    {
        private Proizvod selectedProizvod = null;
        private bool isEditMode = false;

        public UrediProizvodeWindow()
        {
            InitializeComponent();
            LoadCategories();
            LoadProizvodi();
            UpdateFormTitle(); // Početni naslov
        }

        private void LoadProizvodi()
        {
            var proizvodi = ProizvodService.GetAll();
            ProizvodiGrid.ItemsSource = proizvodi;
        }

        private void LoadCategories()
        {
            var kategorije = KategorijaService.GetAll();
            KategorijaComboBox.ItemsSource = kategorije;
            KategorijaComboBox.DisplayMemberPath = "Naziv";
            KategorijaComboBox.SelectedValuePath = "idKATEGORIJA";
        }

        private void UpdateFormTitle()
        {
            TitleTextBlock.Text = isEditMode
                ? (string)FindResource("EditProducts_FormTitle_Edit")
                : (string)FindResource("EditProducts_FormTitle_Add");
        }

        private string GetString(string key, params object[] args)
        {
            var format = (string)FindResource(key);
            return args.Length > 0 ? string.Format(format, args) : format;
        }

        private void ProizvodiGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProizvodiGrid.SelectedItem is Proizvod p)
            {
                selectedProizvod = p;
                isEditMode = true;
                UpdateFormTitle();

                NazivTextBox.Text = p.Naziv;
                CijenaTextBox.Text = p.Cijena.ToString(System.Globalization.CultureInfo.InvariantCulture);
                SifraTextBox.Text = p.Sifra;
                OpisTextBox.Text = p.Opis;
                KategorijaComboBox.SelectedValue = p.Kategorija_idKATEGORIJA;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProizvod == null)
            {
                MessageBox.Show(GetString("EditProducts_SelectToEdit"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Validacija
            if (string.IsNullOrWhiteSpace(NazivTextBox.Text) ||
                string.IsNullOrWhiteSpace(CijenaTextBox.Text) ||
                string.IsNullOrWhiteSpace(SifraTextBox.Text) ||
                KategorijaComboBox.SelectedItem == null)
            {
                MessageBox.Show(GetString("EditProducts_FillAllFields"), "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Parsing cijene
            if (!decimal.TryParse(CijenaTextBox.Text.Trim(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal cijena))
            {
                MessageBox.Show(GetString("EditProducts_InvalidPrice"), "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Provjera šifre
            var sviProizvodi = ProizvodService.GetAll();
            if (sviProizvodi.Any(p => p.Sifra.Equals(SifraTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase) && p.Id != selectedProizvod.Id))
            {
                MessageBox.Show(GetString("EditProducts_CodeExists"), "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ažuriranje
            selectedProizvod.Naziv = NazivTextBox.Text.Trim();
            selectedProizvod.Cijena = cijena;
            selectedProizvod.Sifra = SifraTextBox.Text.Trim();
            selectedProizvod.Opis = OpisTextBox.Text.Trim();
            selectedProizvod.Kategorija_idKATEGORIJA = (int)KategorijaComboBox.SelectedValue;

            try
            {
                ProizvodService.UpdateProizvod(selectedProizvod);
                MessageBox.Show(GetString("EditProducts_Updated"), "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProizvodi();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetString("EditProducts_ErrorUpdate", ex.Message), "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProizvod == null)
            {
                MessageBox.Show(GetString("EditProducts_SelectToDelete"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                GetString("EditProducts_ConfirmDelete", selectedProizvod.Naziv),
                "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ProizvodService.DeleteProizvod(selectedProizvod.Id);
                    MessageBox.Show(GetString("EditProducts_Deleted"), "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProizvodi();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(GetString("EditProducts_ErrorDelete", ex.Message), "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearForm()
        {
            selectedProizvod = null;
            isEditMode = false;
            UpdateFormTitle();

            NazivTextBox.Clear();
            CijenaTextBox.Clear();
            SifraTextBox.Clear();
            OpisTextBox.Clear();
            KategorijaComboBox.SelectedIndex = -1;
        }
    }
}