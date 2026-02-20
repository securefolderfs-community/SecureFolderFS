using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class PercentageToWidthConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not double percentage)
                return 0d;

            var containerWidth = parameter switch
            {
                VisualElement el => el.Width,
                double d => d,
                _ => -1d
            };
            
            if (containerWidth <= 0)
                return 0d;

            return containerWidth * (percentage / 100d);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
