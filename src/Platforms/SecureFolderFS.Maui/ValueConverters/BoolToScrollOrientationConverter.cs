using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class BoolToScrollOrientationConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool bValue)
                return ScrollOrientation.Vertical;

            return bValue ? ScrollOrientation.Vertical : ScrollOrientation.Both;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
