using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RCP_Drawings_Releaser.Converters
{
    public class NewDrawingToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (bool) value ? (SolidColorBrush)new BrushConverter().ConvertFrom("#7FB5FFB0") : (SolidColorBrush)new BrushConverter().ConvertFrom("#7FA6F2FF");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}