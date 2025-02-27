using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class WidthToMarginConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not double dValue)
                return new Thickness();

            dValue = Math.Max(dValue, 0d);
            var strParam = parameter as string;
            var adjusted = AdjustValue(dValue, strParam);

            return strParam?.Contains("right", StringComparison.OrdinalIgnoreCase) ?? false
                ? new Thickness(0d, 0d, adjusted, 0d)
                : new Thickness(adjusted, 0d, 0d, 0d);

            static double AdjustValue(double unadjusted, string? parameter)
            {
                return parameter?.Contains("invert", StringComparison.OrdinalIgnoreCase) ?? false
                    ? -unadjusted
                    : unadjusted;
            }
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
