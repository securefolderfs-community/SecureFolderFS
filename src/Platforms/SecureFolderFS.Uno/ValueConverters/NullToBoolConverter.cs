using Microsoft.UI.Xaml.Data;
using SecureFolderFS.UI.ValueConverters;
using System;

namespace SecureFolderFS.Uno.ValueConverters
{
    /// <inheritdoc cref="BaseNullToBoolConverter"/>
    internal sealed class NullToBoolConverter : BaseNullToBoolConverter, IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            return TryConvert(value, targetType, parameter);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TryConvertBack(value, targetType, parameter);
        }
    }
}
