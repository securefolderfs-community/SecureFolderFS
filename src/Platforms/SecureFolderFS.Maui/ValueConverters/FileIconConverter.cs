using System.Globalization;
using MauiIcons.Core;
using MauiIcons.Material;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using IImage = SecureFolderFS.Shared.ComponentModel.IImage;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class FileIconConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not View { BindingContext: IWrapper<IStorable> storableWrapper })
                return null;

            return value switch
            {
                IImage image => ToImage(image),
                _ => storableWrapper switch
                {
                    { Inner: IFolder } => new MauiIcon() { Icon = MaterialIcons.Folder },
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
