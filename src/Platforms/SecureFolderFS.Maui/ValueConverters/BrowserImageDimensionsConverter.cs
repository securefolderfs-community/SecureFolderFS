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

            var indents = strParam.Split(',', 3);
            var smallStr = indents.FirstOrDefault() ?? "0";
            var mediumStr = indents.ElementAtOrDefault(1) ?? smallStr;
            var largeStr = indents.LastOrDefault() ?? mediumStr;

            var small = System.Convert.ToDouble(smallStr);
            var medium = System.Convert.ToDouble(mediumStr);
            var large = System.Convert.ToDouble(largeStr);

            return viewType switch
            {
                BrowserViewType.SmallGridView or BrowserViewType.SmallGalleryView => small,
                BrowserViewType.MediumGridView or BrowserViewType.MediumGalleryView => medium,
                BrowserViewType.LargeGridView or BrowserViewType.LargeGalleryView => large,
                _ => 0d
            };

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
