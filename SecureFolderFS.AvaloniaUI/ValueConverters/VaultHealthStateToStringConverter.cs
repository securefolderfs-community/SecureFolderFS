using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.AvaloniaUI.ValueConverters
{
    internal sealed class VaultHealthStateToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not VaultHealthState vaultHealthState)
            {
                return string.Empty;
            }

            // TODO: Localize
            return vaultHealthState switch
            {
                VaultHealthState.Healthy => "No problems found",
                VaultHealthState.NeedsAttention => "Needs attention",
                VaultHealthState.Error => "Problems found",
                _ => throw new ArgumentOutOfRangeException(nameof(vaultHealthState))
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
