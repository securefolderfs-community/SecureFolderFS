using Avalonia.Data.Converters;
using SecureFolderFS.UI.ValueConverters;
using System;
using System.Globalization;

namespace SecureFolderFS.AvaloniaUI.ValueConverters
{
    /// <inheritdoc cref="BaseBoolToStringConverter"/>
    internal sealed class BoolToStringConverter : BaseBoolToStringConverter, IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return TryConvert(value, targetType, parameter);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return TryConvertBack(value, targetType, parameter);
        }
    }
}
