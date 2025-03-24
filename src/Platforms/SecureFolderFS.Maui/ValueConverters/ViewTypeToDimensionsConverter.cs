using System.Globalization;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class ViewTypeToDimensionsConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not BrowserViewType viewType)
                return null;
            
            var isHeight = parameter is string strParam && strParam.Equals("height", StringComparison.OrdinalIgnoreCase);
            _ = isHeight;

            return viewType switch
            {
                BrowserViewType.SmallGridView => 30,
                BrowserViewType.MediumGridView => 50,
                BrowserViewType.LargeGridView => 80,
                
                BrowserViewType.LargeGalleryView => 80,
                BrowserViewType.MediumGalleryView => 50,
                BrowserViewType.SmallGalleryView => 30,
                
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
