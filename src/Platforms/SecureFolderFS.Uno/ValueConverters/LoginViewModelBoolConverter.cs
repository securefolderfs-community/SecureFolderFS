using System;
using Microsoft.UI.Xaml.Data;
using SecureFolderFS.UI.ValueConverters;

namespace SecureFolderFS.Uno.ValueConverters
{
    /// <inheritdoc cref="BaseLoginViewModelBoolConverter"/>
    internal sealed class LoginViewModelBoolConverter : BaseLoginViewModelBoolConverter, IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object? value, Type targetType, object parameter, string language)
        {
            if (parameter is not string strParam)
                return false;

            var split = strParam.Split('|');
            var invert = split[1].Contains("invert", StringComparison.OrdinalIgnoreCase);
            var result = false;

            foreach (var item in split[0].Split(','))
            {
                result = value?.GetType().Name.Equals(item, StringComparison.OrdinalIgnoreCase) ?? false;
                if (result)
                    break;
            }

            return invert ? !result : result;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
