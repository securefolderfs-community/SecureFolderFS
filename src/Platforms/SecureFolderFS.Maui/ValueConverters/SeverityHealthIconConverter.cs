using System.Globalization;
using SecureFolderFS.UI.ValueConverters;

namespace SecureFolderFS.Maui.ValueConverters
{
    /// <inheritdoc cref="BaseSeverityHealthIconConverter"/>
    public sealed class SeverityHealthIconConverter : BaseSeverityHealthIconConverter, IValueConverter
    {
        /// <inheritdoc/>
        protected override object? SuccessIcon { get; } =
#if IOS
            ImageSource.FromFile("success_shield.png");
#else
            ImageSource.FromFile("success_shield.svg");
#endif

        /// <inheritdoc/>
        protected override object? WarningIcon { get; } = 
#if IOS
            ImageSource.FromFile("warning_shield.png");
#else
            ImageSource.FromFile("warning_shield.svg");
#endif

        /// <inheritdoc/>
        protected override object? CriticalIcon { get; } =
#if IOS
            ImageSource.FromFile("error_shield.png");
#else
            ImageSource.FromFile("error_shield.svg");
#endif

        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return TryConvert(value, targetType, parameter);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return TryConvertBack(value, targetType, parameter);
        }
    }
}
