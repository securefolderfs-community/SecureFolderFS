using System.Globalization;
using IImage = SecureFolderFS.Shared.ComponentModel.IImage;

namespace SecureFolderFS.Maui.ValueConverters
{
    /// <summary>
    /// Returns AspectFill when a real thumbnail (IImage) is present,
    /// otherwise AspectFit for static fallback icons (folder/file/PDF/etc.).
    /// </summary>
    internal sealed class ThumbnailToAspectConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is IImage ? Aspect.AspectFill : Aspect.AspectFit;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
