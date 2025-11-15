using System.Globalization;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class BrowserImageDimensionsConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not BrowserViewType viewType)
                return 0d;

            if (parameter is not string strParam)
                return 0d;

            // Get columns for each indent
            var indents = strParam.Split(',', 3);
            var smallStr = indents.FirstOrDefault() ?? "3";
            var mediumStr = indents.ElementAtOrDefault(1) ?? smallStr;
            var largeStr = indents.LastOrDefault() ?? mediumStr;

            var small = System.Convert.ToDouble(smallStr);
            var medium = System.Convert.ToDouble(mediumStr);
            var large = System.Convert.ToDouble(largeStr);

            // Get columns count
            var columns = viewType switch
            {
                BrowserViewType.SmallGridView or BrowserViewType.SmallGalleryView => small,
                BrowserViewType.MediumGridView or BrowserViewType.MediumGalleryView => medium,
                BrowserViewType.LargeGridView or BrowserViewType.LargeGalleryView => large,
                _ => 3
            };

            // Calculate dimensions with spacing consideration
            var spacing = 0;
            var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            var availableWidth = screenWidth - (spacing * (columns + 1));
            var itemWidth = availableWidth / columns;

            return itemWidth;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
