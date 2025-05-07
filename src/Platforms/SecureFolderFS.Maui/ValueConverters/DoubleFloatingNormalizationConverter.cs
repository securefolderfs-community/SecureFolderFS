using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class DoubleFloatingNormalizationConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not double dValue)
                return 0d;
            
            var normalization = 1d;
            if (parameter is string strParam && double.TryParse(strParam, CultureInfo.InvariantCulture, out var dParam))
                normalization = dParam;

            return dValue * normalization;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
