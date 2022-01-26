using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace SecureFolderFS.WinUI.ValueConverters
{
    internal sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool boolParam)
            {
                return Visibility.Collapsed;
            }

            if (parameter is not string stringParam)
            {
                return boolParam ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                if (stringParam.ToLower() == "invert")
                {
                    return boolParam ? Visibility.Collapsed : Visibility.Visible;
                }

                return boolParam ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
