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
    /// <summary>
    /// Converts file-related image data into an appropriate <see cref="ImageSource"/>
    /// representation for display purposes. This converter is designed to handle
    /// file type determination and generate corresponding icons or thumbnails.
    /// </summary>
    internal sealed class FileIconConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not View { BindingContext: IWrapper<IStorable> storableWrapper })
                return ImageSource.FromFile(GetDefaultFileIcon());

            // Thumbnail loaded - return optimized ImageSource
            if (value is IImage image)
                return FromImage(image);

            // Fallback icon (folder, file, PDF, archive, etc.)
            return GetFallbackImageSource(storableWrapper);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static ImageSource FromImage(IImage image)
        {
            switch (image)
            {
                case StreamImageModel { Inner.CanRead: true } sim:
                {
                    sim.Inner.TrySetPositionOrAdvance(0L);
                    return new StreamImageSource
                    {
                        Stream = _ =>
                        {
                            sim.Inner.TrySetPositionOrAdvance(0L);
                            return Task.FromResult(sim.Inner);
                        }
                    };
                }

                case ImageStreamSource { Inner.CanRead: true } iss:
                {
                    iss.Inner.TrySetPositionOrAdvance(0L);
                    return iss.Source;
                }

                default:
                    return ImageSource.FromFile(GetDefaultFileIcon());
            }
        }

        private static ImageSource GetFallbackImageSource(IWrapper<IStorable> wrapper)
        {
            // Special PDF handling
            if (wrapper is FileViewModel { Classification.MimeType: "application/pdf" } ||
                wrapper is SearchBrowserItemViewModel { Classification.MimeType: "application/pdf" })
                return ImageSource.FromFile("pdf_icon.png");

            // Platform-specific archive icons
            if (wrapper is FileViewModel { Classification.TypeHint: TypeHint.Archive } ||
                wrapper is SearchBrowserItemViewModel { Classification.TypeHint: TypeHint.Archive })
                return ImageSource.FromFile(GetArchiveIcon());

            // Folder and file
            return wrapper switch
            {
                { Inner: IFolder } => ImageSource.FromFile(GetFolderIcon()),
                _ => ImageSource.FromFile(GetDefaultFileIcon())
            };
        }

        private static string GetArchiveIcon() =>
#if ANDROID
            "android_archive.png";
#else
            "ios_archive.png";
#endif

        private static string GetFolderIcon() =>
#if ANDROID
            "android_folder.png";
#else
            "ios_folder.png";
#endif

        private static string GetDefaultFileIcon() =>
#if ANDROID
            "android_file.png";
#else
            "ios_file.png";
#endif
    }
}
