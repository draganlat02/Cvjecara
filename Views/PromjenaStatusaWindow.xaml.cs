using System.Windows;
using System.Windows.Controls;

namespace PrviProjekat.Views
{
    public partial class PromjenaStatusaWindow : Window
    {
        public string NoviStatus { get; private set; } = "";

        public PromjenaStatusaWindow(string trenutniStatus)
        {
            InitializeComponent();

            // ✅ Uvijek automatski izabrana "Završena"
            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == "Završeno")
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void Sacuvaj_Click(object sender, RoutedEventArgs e)
        {
            NoviStatus = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            DialogResult = true;
            Close();
        }

        private void Odustani_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
