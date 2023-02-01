using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace SecureFolderFS.AvaloniaUI.ValueConverters
{
    internal sealed class OnlyFirstElementOrNullConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IEnumerable enumerable)
            {
                var list = enumerable.OfType<object?>().ToList();
                if (list.Count == 1)
                    return list.FirstOrDefault();
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}