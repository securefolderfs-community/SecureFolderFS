using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;
using System.Linq;

namespace SecureFolderFS.WinUI.ValueConverters
{
    internal sealed class OnlyFirstElementOrNullConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IEnumerable enumerable)
            {
                var list = enumerable.OfType<object?>().ToList();
                if (list.Count == 1)
                    return list.FirstOrDefault();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
