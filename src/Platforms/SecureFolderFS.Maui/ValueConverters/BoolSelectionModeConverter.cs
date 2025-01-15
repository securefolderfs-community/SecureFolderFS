using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class BoolSelectionModeConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool bValue)
                return SelectionMode.None;

            return bValue ? SelectionMode.Multiple : SelectionMode.None;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
