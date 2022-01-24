using System;
using Microsoft.UI.Xaml.Data;

#nullable enable

namespace SecureFolderFS.WinUI.ValueConverters
{
    internal sealed class DateTimeToStringValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not DateTime dateTime)
            {
                return string.Empty;
            }

            if (dateTime.Date == DateTime.Today)
            {
                return dateTime.ToString("Today, HH:mm");
            }
            else
            {
                return dateTime.ToString("MM/dd/yyyy, HH:mm");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
