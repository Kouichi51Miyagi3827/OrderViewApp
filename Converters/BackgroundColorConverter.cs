using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OrderViewApp.Converters
{
    public class BackgroundColorConverter : IMultiValueConverter
    {
        // 見やすい背景色のパレット（文字がしっかり見える薄い色調）
        private static readonly Brush[] BackgroundColors = new Brush[]
        {
            new SolidColorBrush(Color.FromRgb(255, 255, 255)), // 白（デフォルト）
            new SolidColorBrush(Color.FromRgb(230, 240, 255)), // 薄い青
            new SolidColorBrush(Color.FromRgb(255, 240, 230)), // 薄いオレンジ
            new SolidColorBrush(Color.FromRgb(240, 255, 240)), // 薄い緑
            new SolidColorBrush(Color.FromRgb(255, 255, 240)), // 薄い黄
            new SolidColorBrush(Color.FromRgb(240, 240, 255)), // 薄い紫
            new SolidColorBrush(Color.FromRgb(255, 240, 255)), // 薄いピンク
            new SolidColorBrush(Color.FromRgb(240, 255, 255)), // 薄いシアン
            new SolidColorBrush(Color.FromRgb(255, 245, 230)), // 薄いベージュ
            new SolidColorBrush(Color.FromRgb(245, 255, 245)), // 薄いミントグリーン
        };

        // グレーの背景色
        private static readonly Brush GrayBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240));

        // 水色の背景色
        private static readonly Brush LightBlueBrush = new SolidColorBrush(Color.FromRgb(230, 240, 255));

        // 緑の背景色
        private static readonly Brush LightGreenBrush = new SolidColorBrush(Color.FromRgb(240, 255, 240));

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                return BackgroundColors[0]; // デフォルトは白
            }

            // 値の取得
            int? groupColorIndex = values[0] as int?; // GroupColorIndex
            bool isBackgroundColorEnabled = values[1] is bool enabled && enabled;
            int alternationIndex = values[2] is int altIndex ? altIndex : -1;

            // 背景色が無効の場合：白とグレーを交互に
            if (!isBackgroundColorEnabled)
            {
                return alternationIndex % 2 == 0 ? BackgroundColors[0] : GrayBrush;
            }

            // 背景色が有効の場合：GroupColorIndexに基づいて色を返す
            if (groupColorIndex.HasValue)
            {
                // GroupColorIndexが0なら水色、1なら緑を返す
                return groupColorIndex.Value == 0 ? LightBlueBrush : LightGreenBrush;
            }

            // NULLまたは値がない場合はデフォルトの白色
            return BackgroundColors[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

