using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class BoolToSelectionBackgroundConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool?)value ?? false
                ? Application.Current?.Resources["SecondaryBrush"] as SolidColorBrush
                : new SolidColorBrush(Colors.Transparent);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
