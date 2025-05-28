using System.Globalization;
using MauiIcons.Core;
using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
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

#if ANDROID
                _ => storableWrapper switch
                {
                    { Inner: IFolder } => new MauiIcon() { HorizontalOptions = LayoutOptions.Center, Icon = MauiIcons.Material.MaterialIcons.Folder, IconAutoScaling = true },
                    _ => new MauiIcon() { HorizontalOptions = LayoutOptions.Center, Icon = MauiIcons.Material.MaterialIcons.Description, IconAutoScaling = true }
                }
#else
                _ => storableWrapper switch
                {
                    { Inner: IFolder } => new MauiIcon() { Icon = MauiIcons.Cupertino.CupertinoIcons.Folder },
                    _ => new MauiIcon() { Icon = MauiIcons.Cupertino.CupertinoIcons.Doc }
                }
#endif
            };

            static object? ToImage(IImage image)
            {
                switch (image)
                {
                    case ImageStream { Stream.CanRead: true } imageStream:
                    {
                        imageStream.Stream.Position = 0L;
                        return new Image() { Source = imageStream.Source, Aspect = Aspect.AspectFill };
                    }

                    default: return null; // TODO: Add more IImage implementations
                }
            }
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
