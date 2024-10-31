using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using SecureFolderFS.UI.ValueConverters;

namespace SecureFolderFS.Uno.ValueConverters
{
    internal sealed class SeverityHealthIconConverter : BaseSeverityHealthIconConverter, IValueConverter
    {
        /// <inheritdoc/>
        protected override object? SuccessIcon { get; } =
#if WINDOWS && !HAS_UNO
            new SvgImageSource(new("ms-appx://SecureFolderFS.UI/Assets/AppAssets/green_shield.svg"));
#else
            new SvgImageSource(new("/Assets/AppAssets/green_shield.png"));
#endif

        /// <inheritdoc/>
        protected override object? WarningIcon { get; } = null;

        /// <inheritdoc/>
        protected override object? CriticalIcon { get; } = null;

        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            return TryConvert(value, targetType, parameter);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TryConvertBack(value, targetType, parameter);
        }
    }
}
