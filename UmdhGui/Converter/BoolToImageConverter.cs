using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UmdhGui.Converter
{
    public class BoolToImageConverter : IValueConverter
    {
        public ImageSource TrueImage { get; set; }
        public ImageSource FalseImage { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return null;

            var boolValue = (bool) value;
            if (boolValue)
            {
                return TrueImage;
            }
            return FalseImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}