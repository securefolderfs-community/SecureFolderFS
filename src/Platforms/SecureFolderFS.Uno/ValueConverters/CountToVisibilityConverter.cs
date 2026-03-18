using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using SecureFolderFS.UI.ValueConverters;

namespace SecureFolderFS.Uno.ValueConverters
{
    /// <inheritdoc cref="BaseCountToBoolConverter"/>
    internal sealed class CountToVisibilityConverter : BaseCountToBoolConverter, IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var result = TryConvert(value, targetType, parameter) as bool? ?? false;
            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var result = TryConvertBack(value, targetType, parameter) as bool? ?? false;
            return result ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
