using System.Globalization;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class ViewTypeToItemsLayoutConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not BrowserViewType viewType)
                return null;

            return viewType switch
            {
                BrowserViewType.SmallGridView => new GridItemsLayout(5, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 8d, HorizontalItemSpacing = 8d },
                BrowserViewType.MediumGridView => new GridItemsLayout(3, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 8d, HorizontalItemSpacing = 8d },
                BrowserViewType.LargeGridView => new GridItemsLayout(2, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 8d, HorizontalItemSpacing = 8d },

                BrowserViewType.SmallGalleryView => new GridItemsLayout(8, ItemsLayoutOrientation.Vertical),
                BrowserViewType.MediumGalleryView => new GridItemsLayout(5, ItemsLayoutOrientation.Vertical),
                BrowserViewType.LargeGalleryView => new GridItemsLayout(3, ItemsLayoutOrientation.Vertical),

                BrowserViewType.ColumnView => new GridItemsLayout(2, ItemsLayoutOrientation.Vertical),
                _ => new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            };
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
