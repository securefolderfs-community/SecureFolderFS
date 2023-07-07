using Avalonia.Data.Converters;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using System;
using System.Globalization;

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
                VaultHealthState.Healthy => "HealthNoProblems".ToLocalized(),
                VaultHealthState.NeedsAttention => "HealthAttention".ToLocalized(),
                VaultHealthState.Error => "HealthProblems".ToLocalized(),
                _ => throw new ArgumentOutOfRangeException(nameof(vaultHealthState))
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
