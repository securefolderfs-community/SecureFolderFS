using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class BoolOpacityConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool bValue)
                return 0d;

            if (parameter is string strParam && strParam.Equals("invert", StringComparison.OrdinalIgnoreCase))
                return bValue ? 0d : 1d;

            return bValue ? 1d : 0d;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
