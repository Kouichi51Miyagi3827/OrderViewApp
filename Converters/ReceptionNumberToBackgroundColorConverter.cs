using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OrderViewApp.Converters
{
    public class ReceptionNumberToBackgroundColorConverter : IValueConverter
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

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? receptionNumber = value as int?;
            if (receptionNumber.HasValue)
            {
                // ハッシュ関数を使用して色を選択
                // 2654435761は2^32 × φ（黄金比）に近い素数で、良い分散を提供
                // 連続する受付番号でも異なる色になるようにする
                // 同じ受付番号は常に同じハッシュ値になるため、同じ色が保証される
                const uint hashMultiplier = 2654435761u;
                long hash = ((long)receptionNumber.Value * hashMultiplier) & 0xFFFFFFFFL;
                int index = (int)(Math.Abs(hash) % (BackgroundColors.Length - 1));
                // 0番目（白）はスキップして、1番目から使用
                return BackgroundColors[index + 1];
            }

            // NULLまたは値がない場合はデフォルトの白色
            return BackgroundColors[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

