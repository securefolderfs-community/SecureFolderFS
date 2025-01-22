using System;
using Microsoft.UI.Xaml.Data;

namespace SecureFolderFS.Uno.ValueConverters
{
    internal sealed class CountToBoolConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not int numValue)
                return false;

            return numValue > 0;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
