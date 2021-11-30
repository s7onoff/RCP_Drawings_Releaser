﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RCP_Drawings_Releaser.Converters
{
    public class HasNumProblemsToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (bool) value ? new SolidColorBrush(Colors.Red) : (SolidColorBrush)Application.Current.FindResource("TextBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}