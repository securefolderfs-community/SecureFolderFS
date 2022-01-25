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

            string dateString;
            if (dateTime.Date == DateTime.Today)
            {
                dateString = dateTime.ToString("Today, HH:mm"); // TODO: Localize
            }
            else
            {
                dateString = dateTime.ToString("MM/dd/yyyy, HH:mm");
            }

            if (parameter is string formatString)
            {
                var split = formatString.Split('|');
                if (split[0] == "LOCALIZE")
                {
                    return string.Format(split[1], dateString); // TODO: Localize
                }
                else
                {
                    return string.Format(split[1], dateString);
                }
            }

            return dateString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
