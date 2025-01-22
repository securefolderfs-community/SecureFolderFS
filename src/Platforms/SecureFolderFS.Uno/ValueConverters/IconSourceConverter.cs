using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using SecureFolderFS.Uno.AppModels;

namespace SecureFolderFS.Uno.ValueConverters
{
    internal sealed class IconSourceConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                ImageBitmap bitmap => new ImageIconSource() { ImageSource = bitmap.Source },
                _ => GetDefault()
            };

            IconSource? GetDefault()
            {
                if (parameter is not string strParam)
                    return null;

                return new FontIconSource() { Glyph = strParam };
            }
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
