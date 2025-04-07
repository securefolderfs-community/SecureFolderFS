using System.Globalization;
using Microsoft.Maui.Graphics.Converters;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class BoolToSelectionBackgroundConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not string strParam)
            {
                return (bool?)value ?? false
                    ? Application.Current?.Resources["SecondaryBrush"] as SolidColorBrush
                    : new SolidColorBrush(Colors.Transparent);
            }

            var split = strParam.Split('|', 2);
            var first = split.ElementAtOrDefault(0);
            var second = split.ElementAtOrDefault(1);

            Brush? firstBrush = null;
            Brush? secondBrush = null;
            var colorConverter = new ColorTypeConverter();
            if (!string.IsNullOrEmpty(first))
            {
                firstBrush = Application.Current?.Resources.Get(first) as Brush;
                if (firstBrush is null)
                {
                    if (colorConverter.ConvertFromInvariantString(first) is Color color)
                        firstBrush = new SolidColorBrush(color);
                }
            }

            if (!string.IsNullOrEmpty(second))
            {
                secondBrush = Application.Current?.Resources.Get(second) as Brush;
                if (secondBrush is null)
                {
                    if (colorConverter.ConvertFromInvariantString(second) is Color color)
                        secondBrush = new SolidColorBrush(color);
                }
            }

            return (bool?)value ?? false
                ? (firstBrush ?? Application.Current?.Resources["SecondaryBrush"] as SolidColorBrush)
                : (secondBrush ?? new SolidColorBrush(Colors.Transparent));
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
