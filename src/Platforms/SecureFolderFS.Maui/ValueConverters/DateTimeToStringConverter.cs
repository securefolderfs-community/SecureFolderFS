using System.Globalization;
using SecureFolderFS.UI.ValueConverters;

namespace SecureFolderFS.Maui.ValueConverters
{
    /// <inheritdoc cref="BaseDateTimeToStringConverter"/>
    public sealed class DateTimeToStringConverter : BaseDateTimeToStringConverter, IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return base.TryConvert(value, targetType, parameter);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return base.TryConvertBack(value, targetType, parameter);
        }
    }
}
