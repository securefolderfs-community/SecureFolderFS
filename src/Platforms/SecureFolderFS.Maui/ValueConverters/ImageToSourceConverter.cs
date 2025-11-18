using System.ComponentModel;
using System.Globalization;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class ImageToSourceConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                ImageStream imageStream => imageStream.Source,
                ImageIcon iconImage => new FontImageSource()
                {
                    Glyph = GetDescription(iconImage.MauiIcon.Icon),
                    Color = ((Color?)iconImage.MauiIcon.IconColor ?? App.Instance.Resources[MauiThemeHelper.Instance.CurrentTheme switch
                    {
                        ThemeType.Light => "QuarternaryLightColor",
                        _ => "QuarternaryDarkColor"
                    }] as Color)!,
                    FontFamily = iconImage.MauiIcon.Icon?.GetType().Name,
                },
                Uri uriImage => ImageSource.FromUri(uriImage),
                ImageRemoteUrl remoteUrlImage => ImageSource.FromUri(new Uri(remoteUrlImage.Url)),
                ImageResourceFile resourceFileImage => resourceFileImage.IsResource
                    ? ImageSource.FromResource(resourceFileImage.Name)
                    : ImageSource.FromFile(resourceFileImage.Name),
                _ => null
            };
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string GetDescription(Enum? value)
        {
            var fieldInfo = value?.GetType().GetField(value.ToString());
            if (fieldInfo is null)
                return string.Empty;

            var attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            return attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : string.Empty;
        }
    }
}
