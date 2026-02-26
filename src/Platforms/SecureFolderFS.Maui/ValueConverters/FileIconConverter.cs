using System.Globalization;
using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
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
                IImage image => FromImage(image),
                null when storableWrapper is FileViewModel { Classification.MimeType: "application/pdf" } => new Image() { Source = "pdf_icon.png" },
#if ANDROID
                _ => storableWrapper switch
                {
                    { Inner: IFolder } => new Image() { Source = "android_folder.png" },
                    _ => new Image() { Source = "android_file.png" }
                }
#else
                _ => storableWrapper switch
                {
                    { Inner: IFolder } => new Image() { Source = "ios_folder.png" },
                    _ => new Image() { Source = "ios_file.png" }
                }
#endif
            };

            static object? FromImage(IImage image)
            {
                switch (image)
                {
                    case StreamImageModel { Stream.CanRead: true } streamImageModel:
                    {
                        streamImageModel.Stream.TrySetPositionOrAdvance(0L);
                        return new Image()
                        {
                            Source = new StreamImageSource()
                            {
                                Stream = _ => Task.FromResult(streamImageModel.Stream)
                            },
                            Aspect = Aspect.AspectFill,
                            HorizontalOptions = LayoutOptions.Fill,
                            VerticalOptions = LayoutOptions.Fill
                        };
                    }

                    case ImageStream { Stream.CanRead: true } imageStream:
                    {
                        imageStream.Stream.TrySetPositionOrAdvance(0L);
                        return new Image()
                        {
                            Source = imageStream.Source,
                            Aspect = Aspect.AspectFill,
                            HorizontalOptions = LayoutOptions.Fill,
                            VerticalOptions = LayoutOptions.Fill
                        };
                    }

                    default: return null;
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
