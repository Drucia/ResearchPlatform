using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace ResearchPlatform.Helpers
{
    [ValueConversion(typeof(string), typeof(string))]
    public class MatrixItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string comparision = value as string;
            var splitted = comparision.Split("/");
            if (splitted.Length > 1)
            {
                return splitted[1];
            }
            return $"1/{comparision}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
