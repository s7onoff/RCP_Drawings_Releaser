using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RCP_Drawings_Releaser.Converters
{
    public class SheetRevNumToColorConverter : IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (bool) value ? new SolidColorBrush(Colors.DarkBlue) : new SolidColorBrush(Colors.SlateGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var sheetNum = (bool)values[0];
            var revNum = (bool)values[1];

            Color color = new Color();
            
            switch (sheetNum)
            {
                case true when revNum == false:
                {
                    color = Colors.LawnGreen;
                    break;
                }
                case true when revNum == true:
                {
                    color = Colors.Red;
                    break;
                }
                case false when revNum == false:
                {
                    color = Colors.LightGray;
                    break;
                }
                case false when revNum == true:
                {
                    color = Colors.DodgerBlue;
                    break;
                }
            }

            return new SolidColorBrush(color);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}