using System.Globalization;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class BreadcrumbConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not string strParam)
                return null;

            switch (strParam.ToLower())
            {
                case "fontattributes":
                    if (value is not bool bValue)
                        return FontAttributes.None;
                    
                    return bValue ? FontAttributes.Bold : FontAttributes.None;
            }

            return null;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
