using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class FolderItemsCountToStringConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int intValue)
                return null;

            // TODO: Localize text (use different strings for singular and plural forms)
            return $"{intValue} elements"; 
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
