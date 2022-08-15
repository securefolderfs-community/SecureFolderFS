using System;
using Microsoft.UI.Xaml.Data;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.WinUI.ValueConverters
{
    internal sealed class VaultHealthStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
