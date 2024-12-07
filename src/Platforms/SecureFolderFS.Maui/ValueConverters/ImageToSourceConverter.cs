using System.Globalization;
using SecureFolderFS.Maui.AppModels;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class ImageToSourceConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                ImageStream imageStream => imageStream.Source,
                _ => null
            };
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
