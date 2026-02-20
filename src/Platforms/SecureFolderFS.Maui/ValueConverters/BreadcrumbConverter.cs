using System.Globalization;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class BreadcrumbConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not string strParam)
                return null;
            
            var isDark = MauiThemeHelper.Instance.ActualTheme == ThemeType.Dark;
            switch (strParam.ToLowerInvariant())
            {
                case "fontattributes":
                {
                    if (value is not bool bFontValue)
                        return FontAttributes.None;

                    return bFontValue ? FontAttributes.Bold : FontAttributes.None;
                }

                case "background":
                {
                    return value is not true
                        ? Colors.Transparent
                        : App.Current.Resources[isDark ? "SecondaryDarkColor" : "SecondaryLightColor"];
                }

                case "textcolor":
                {
                    if (value is not bool bTextValue)
                        return App.Current.Resources[isDark ? "TertiaryDarkColor" : "TertiaryLightColor"];

                    return bTextValue
                        ? App.Current.Resources[isDark ? "PrimaryDarkColor" : "PrimaryLightColor"]
                        : App.Current.Resources[isDark ? "QuarternaryDarkColor" : "QuarternaryLightColor"];
                }

                case "labelopacity":
                {
                    if (value is not bool bOpacityValue)
                        return 1.0;

                    return bOpacityValue ? 1.0 : 0.75;
                }
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
