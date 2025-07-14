using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class VisibilityToColumnSpanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool bValue)
                return 0;

            if (parameter is string strParam && strParam.Equals("column", StringComparison.OrdinalIgnoreCase))
                return bValue ? 1 : 0;

            return bValue ? 1 : 2;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
