using System.Globalization;
using OwlCore.Storage;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
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
                null when storableWrapper is SearchBrowserItemViewModel { Classification.MimeType: "application/pdf" } => new Image() { Source = "pdf_icon.png" },

#if ANDROID
                null when storableWrapper is FileViewModel { Classification.TypeHint: TypeHint.Archive } => new Image() { Source = "android_archive.png" },
                null when storableWrapper is SearchBrowserItemViewModel { Classification.TypeHint: TypeHint.Archive } => new Image() { Source = "android_archive.png" },
                _ => storableWrapper switch
                {
                    { Inner: IFolder } => new Image() { Source = "android_folder.png", Margin = new(0d, 0d, -8d, 0d)},
                    _ => new Image() { Source = "android_file.png", Scale = 0.8f }
                }
#else
                null when storableWrapper is FileViewModel { Classification.TypeHint: TypeHint.Archive } => new Image() { Source = "ios_archive.png" },
                null when storableWrapper is SearchBrowserItemViewModel { Classification.TypeHint: TypeHint.Archive } => new Image() { Source = "ios_archive.png" },
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
                    case StreamImageModel { Inner.CanRead: true } streamImageModel:
                    {
                        streamImageModel.Inner.TrySetPositionOrAdvance(0L);
                        return new Image()
                        {
                            Source = new StreamImageSource()
                            {
                                Stream = _ => Task.FromResult(streamImageModel.Inner)
                            },
                            Aspect = Aspect.AspectFill,
                            HorizontalOptions = LayoutOptions.Fill,
                            VerticalOptions = LayoutOptions.Fill
                        };
                    }

                    case ImageStreamSource { Inner.CanRead: true } imageStream:
                    {
                        imageStream.Inner.TrySetPositionOrAdvance(0L);
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
