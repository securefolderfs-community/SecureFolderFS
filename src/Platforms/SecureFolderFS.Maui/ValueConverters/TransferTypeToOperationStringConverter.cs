using System.Globalization;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class TransferTypeToOperationStringConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not TransferType transferType)
                return "Transfer";

            return transferType switch
            {
                TransferType.Copy => "Copy".ToLocalized(),
                TransferType.Move => "Move".ToLocalized(),
                TransferType.Select => "Select".ToLocalized(),
                _ => "Transfer"
            };
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
