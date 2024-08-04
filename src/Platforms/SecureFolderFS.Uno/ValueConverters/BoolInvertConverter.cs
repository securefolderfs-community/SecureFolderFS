using System;
using Microsoft.UI.Xaml.Data;

namespace SecureFolderFS.Uno.ValueConverters
{
    internal sealed class BoolInvertConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool boolVal)
                return false;

            if (parameter is string strParam && strParam.ToLower() == "invert")
                return boolVal;

            return !boolVal;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
