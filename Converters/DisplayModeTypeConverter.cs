using System;
using System.Globalization;
using System.Windows.Data;
using OrderViewApp.Models;

namespace OrderViewApp.Converters
{
    public class DisplayModeTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DisplayModeType displayMode)
            {
                return displayMode switch
                {
                    DisplayModeType.Incomplete => "準備未完了",
                    DisplayModeType.All => "完了含む",
                    DisplayModeType.Completed => "完了のみ",
                    _ => value.ToString() ?? string.Empty
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

