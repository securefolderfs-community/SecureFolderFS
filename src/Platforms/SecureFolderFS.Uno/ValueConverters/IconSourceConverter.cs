using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using SecureFolderFS.UI.AppModels;
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
                ImageGlyph glyph => glyph switch
                {
                    { FontFamily: not null } => new FontIconSource() { Glyph = glyph.Glyph, FontFamily = new FontFamily(glyph.FontFamily) },
                    _ => new FontIconSource() { Glyph = glyph.Glyph },
                },
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
