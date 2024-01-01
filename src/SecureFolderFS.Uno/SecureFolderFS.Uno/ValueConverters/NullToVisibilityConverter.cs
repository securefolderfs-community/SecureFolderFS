using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace SecureFolderFS.Uno.ValueConverters
{
    internal sealed class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string strParam && strParam.ToLower() == "invert")
            {
                if (value is string str1)
                {
                    return string.IsNullOrEmpty(str1) ? Visibility.Visible : Visibility.Collapsed;
                }

                return value is null ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is string str)
            {
                return !string.IsNullOrEmpty(str) ? Visibility.Visible : Visibility.Collapsed;
            }

            return value is not null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
