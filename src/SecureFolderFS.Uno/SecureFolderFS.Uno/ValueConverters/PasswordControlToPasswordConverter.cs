using Microsoft.UI.Xaml.Data;
using SecureFolderFS.Uno.UserControls;
using System;

namespace SecureFolderFS.Uno.ValueConverters
{
    internal sealed class PasswordControlToPasswordConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PasswordControl passwordControl)
                return passwordControl.GetPassword();

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
