using System.Globalization;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class BrowserItemTypeToBoolConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not BrowserItemViewModel itemViewModel)
                return false;

            if (parameter is not string strParam)
                return false;

            var typeHint = FileTypeHelper.GetTypeHint(itemViewModel.Inner);
            return strParam.Equals(typeHint.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
