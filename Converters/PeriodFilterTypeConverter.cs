using System;
using System.Globalization;
using System.Windows.Data;
using OrderViewApp.Models;

namespace OrderViewApp.Converters
{
    public class PeriodFilterTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PeriodFilterType filterType)
            {
                return filterType switch
                {
                    PeriodFilterType.Today => "当日",
                    PeriodFilterType.TodayAndTomorrow => "当日＆翌日",
                    PeriodFilterType.Tomorrow => "翌日",
                    PeriodFilterType.Custom => "カスタム期間",
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

