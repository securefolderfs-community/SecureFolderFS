using Microsoft.UI.Xaml.Data;
using System;

namespace SecureFolderFS.WinUI.ValueConverters
{
    internal sealed class IndexToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not int currentIndex)
                return 0.0d;

            var strParam = (string?)parameter;
            if (strParam is null || !int.TryParse(strParam, out var progressIndex))
                return 0.0d;

            progressIndex += 1;

            var progval = progressIndex * 2; // because each prog has 2 states
            var newval = currentIndex - progval;

            if (newval == -1)
                return 50.0d;

            if (newval >= 0)
                return 100.0d;


            return 0.0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
