

using System;
using System.Windows.Data;

namespace MultiThreadedReactiveUI.Converters
{
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string value_str = value.ToString();
            if (String.IsNullOrWhiteSpace(value_str)) return null;

            value_str = value_str.TrimEnd(culture.NumberFormat.PercentSymbol.ToCharArray());

            double result;
            if (Double.TryParse(value_str, out result))
            {
                return result / 100;
            }
            return value;
        }
    }
}

