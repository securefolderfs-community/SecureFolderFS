using System.Globalization;
using SecureFolderFS.UI.ValueConverters;

namespace SecureFolderFS.Maui.ValueConverters
{
    /// <inheritdoc cref="BaseBoolToStringConverter"/>
    internal sealed class BoolToStringConverter : BaseBoolToStringConverter, IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return TryConvert(value, targetType, parameter);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return TryConvertBack(value, targetType, parameter);
        }
    }
}
