using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace SecureFolderFS.Uno.ValueConverters
{
    internal sealed class BoolToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object parameter, string language)
        {
            if (value is not bool bValue)
                return false;

            var invert = parameter is string strParam && strParam.ToLower() == "invert";
            bValue = invert ? !bValue : bValue;

            return bValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
