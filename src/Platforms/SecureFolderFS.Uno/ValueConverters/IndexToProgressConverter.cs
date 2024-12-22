using System;
using Microsoft.UI.Xaml.Data;

namespace SecureFolderFS.Uno.ValueConverters
{
    internal sealed class IndexToProgressConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not int currentIndex)
                return 0.0d;

            var strParam = (string?)parameter;
            if (strParam is null || !int.TryParse(strParam, out var progressIndex))
                return 0.0d;

            progressIndex += 1;

            var progress = progressIndex * 2; // Because each progress has 2 states
            var newValue = currentIndex - progress;

            if (newValue == -1)
                return 50.0d;

            if (newValue >= 0)
                return 100.0d;

            return 0.0d;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
