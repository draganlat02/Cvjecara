using PrviProjekat.Models;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PrviProjekat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        
        public static Korisnik TrenutniKorisnik { get; set; }
    
    public class StringToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                => string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }



        public static void ChangeLanguage(string lang)
        {
            // UKLONI STARI JEZIK
            var oldLang = Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("/Languages/") == true);
            if (oldLang != null)
                Current.Resources.MergedDictionaries.Remove(oldLang);

            // DODAJ NOVI
            var langDict = new ResourceDictionary
            {
                Source = new Uri($"/Languages/{lang}.xaml", UriKind.Relative)
            };
            Current.Resources.MergedDictionaries.Add(langDict);
        }

        public static void ApplyTheme(string tema)
        {
            if (string.IsNullOrEmpty(tema)) tema = "Light";

            var uri = new Uri($"/Themes/{tema}Theme.xaml", UriKind.Relative);
            var dict = new ResourceDictionary { Source = uri };

            // KORISTI Current.Resources (trenutni prozor)
            var currentLang = Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("/Languages/") == true);

            var old = Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("/Themes/") == true);

            if (old != null)
                Current.Resources.MergedDictionaries.Remove(old);

            Current.Resources.MergedDictionaries.Add(dict);

            // Vrati jezik samo ako nije već dodan
            if (currentLang != null && !Current.Resources.MergedDictionaries.Contains(currentLang))
                Current.Resources.MergedDictionaries.Add(currentLang);
        }



    }

  

}
