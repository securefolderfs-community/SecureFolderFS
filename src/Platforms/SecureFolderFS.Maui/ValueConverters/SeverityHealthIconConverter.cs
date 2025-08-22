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
            ImageSource.FromFile("shield_success.png");
#else
            ImageSource.FromFile("shield_success.svg");
#endif

        /// <inheritdoc/>
        protected override object? WarningIcon { get; } = 
#if IOS
            ImageSource.FromFile("shield_warning.png");
#else
            ImageSource.FromFile("shield_warning.svg");
#endif

        /// <inheritdoc/>
        protected override object? CriticalIcon { get; } =
#if IOS
            ImageSource.FromFile("shield_error.png");
#else
            ImageSource.FromFile("shield_error.svg");
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
