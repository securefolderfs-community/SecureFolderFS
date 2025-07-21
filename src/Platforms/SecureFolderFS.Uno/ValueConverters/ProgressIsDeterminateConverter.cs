using System;
using Microsoft.UI.Xaml.Data;

namespace SecureFolderFS.Uno.ValueConverters
{
    internal sealed class ProgressIsDeterminateConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            var strParam = (string)parameter ?? string.Empty;
            var invert = strParam.Contains("invert", StringComparison.OrdinalIgnoreCase);

            if (value is not double progress)
                return invert;

            if (progress == 0d)
                return !invert;

            return invert;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
