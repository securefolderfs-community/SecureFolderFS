using System.Globalization;
using SecureFolderFS.UI.ValueConverters;

namespace SecureFolderFS.Maui.ValueConverters
{
    /// <inheritdoc cref="BaseSeverityHealthIconConverter"/>
    public sealed class SeverityHealthIconConverter : BaseSeverityHealthIconConverter, IValueConverter
    {
        /// <inheritdoc/>
        protected override object? SuccessIcon { get; } = ImageSource.FromFile("success_shield.svg");

        /// <inheritdoc/>
        protected override object? WarningIcon { get; } = ImageSource.FromFile("warning_shield.svg");

        /// <inheritdoc/>
        protected override object? CriticalIcon { get; } = ImageSource.FromFile("error_shield.svg");

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
