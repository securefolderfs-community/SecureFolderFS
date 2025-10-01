using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using SecureFolderFS.UI.ValueConverters;

namespace SecureFolderFS.Uno.ValueConverters
{
    /// <inheritdoc cref="BaseTypeNameBoolConverter"/>
    internal sealed class TypeNameVisibilityConverter : BaseTypeNameBoolConverter, IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object parameter, string language)
        {
            return (bool?)TryConvert(value, targetType, parameter) ?? false ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (bool?)TryConvertBack(value, targetType, parameter) ?? false ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
