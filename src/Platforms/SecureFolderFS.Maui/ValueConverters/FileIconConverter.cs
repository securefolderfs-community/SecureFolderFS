using System.Globalization;
using MauiIcons.Core;
using MauiIcons.Material;
using SecureFolderFS.Sdk.ViewModels.Views.Browser;
using IImage = Microsoft.Maui.IImage;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class FileIconConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not View { BindingContext: BrowserItemViewModel itemViewModel })
                return null;

            return value switch
            {
                IImage image => ToImage(image),
                _ => itemViewModel switch
                {
                    FolderViewModel => new MauiIcon() { Icon = MaterialIcons.Folder },
                    _ => new MauiIcon() { Icon = MaterialIcons.Description }
                }
            };
            
            static object? ToImage(IImage image)
            {
                // TODO: Implement IImage
                return null;
            }
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
