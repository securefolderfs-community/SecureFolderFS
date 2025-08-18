using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class ExpandedStateToIconConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool bValue)
                return null;

#if ANDROID
            return bValue
                ? MauiIcons.Material.MaterialIcons.ArrowDropUp
                : MauiIcons.Material.MaterialIcons.ArrowDropDown;
#elif IOS
            return bValue
                ? MauiIcons.Cupertino.CupertinoIcons.ChevronUp
                : MauiIcons.Cupertino.CupertinoIcons.ChevronDown;
#else
            return null;
#endif
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
