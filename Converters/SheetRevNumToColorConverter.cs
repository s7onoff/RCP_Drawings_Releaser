using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RCP_Drawings_Releaser.Converters
{
    public class SheetRevNumToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var sheetNum = (bool)values[0];
            var revNum = (bool)values[1];

            Color color = new Color();
            
            switch (sheetNum)
            {
                case true when revNum == false:
                {
                    color = (Color)ColorConverter.ConvertFromString("#FAE3D239");
                    break;
                }
                case true when revNum == true:
                {
                    color = Colors.Red;
                    break;
                }
                case false when revNum == false:
                {
                    color = (Color)ColorConverter.ConvertFromString("#FACAE4DF");
                    break;
                }
                case false when revNum == true:
                {
                    color = (Color)ColorConverter.ConvertFromString("#DA6571FF");
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