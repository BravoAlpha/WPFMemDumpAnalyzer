using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPFMemDumpAnalyzer.UI
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var valueAsInt = (int) value;
                return valueAsInt > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}