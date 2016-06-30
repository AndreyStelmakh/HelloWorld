using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfMenu.ValueConverters
{
    [ValueConversion(typeof(string), typeof(bool), ParameterType = typeof(int))]
    public class LessThanXToBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return ((string)value).Length > int.Parse((string)parameter);

            }
            catch { }

            return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();

        }

    }

}
