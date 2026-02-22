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

            return ConvertLayout(viewType);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static ItemsLayout ConvertLayout(BrowserViewType viewType)
        {
            return viewType switch
            {
                BrowserViewType.SmallGridView => new GridItemsLayout(4, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 16d, HorizontalItemSpacing = 16d },
                BrowserViewType.MediumGridView => new GridItemsLayout(3, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 16d, HorizontalItemSpacing = 16d },
                BrowserViewType.LargeGridView => new GridItemsLayout(2, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 16d, HorizontalItemSpacing = 16d },

                BrowserViewType.SmallGalleryView => new GridItemsLayout(7, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 2d, HorizontalItemSpacing = 2d },
                BrowserViewType.MediumGalleryView => new GridItemsLayout(5, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 2d, HorizontalItemSpacing = 2d },
                BrowserViewType.LargeGalleryView => new GridItemsLayout(3, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 2d, HorizontalItemSpacing = 2d },

                BrowserViewType.ColumnView => new GridItemsLayout(2, ItemsLayoutOrientation.Vertical),
                _ => new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            };
        }
    }
}
