using System;
using Microsoft.UI.Xaml.Data;
using SecureFolderFS.UI.ValueConverters;

namespace SecureFolderFS.Uno.ValueConverters
{
    /// <inheritdoc cref="BaseTypeNameBoolConverter"/>
    internal sealed class TypeNameBoolBoolConverter : BaseTypeNameBoolConverter, IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object parameter, string language)
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
