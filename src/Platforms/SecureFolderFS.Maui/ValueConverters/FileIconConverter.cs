using System.Globalization;
using System.Runtime.CompilerServices;
using OwlCore.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Extensions;
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
        // The platform image loader disposes of the stream it is handed after decoding and can
        // request the image again later (recycled cells, re-layouts). Snapshot the bytes once
        // per image instance and serve a fresh stream per request, so a re-bind never hits a
        // stream that has already been consumed and disposed of
        private static readonly ConditionalWeakTable<IImage, byte[]> ImageDataSnapshots = new();

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
            if (image is not IImageStream)
                return ImageSource.FromFile(GetDefaultFileIcon());

            var data = ImageDataSnapshots.GetValue(image, static key => SnapshotBytes(((IImageStream)key).Inner));
            if (data.Length == 0)
                return ImageSource.FromFile(GetDefaultFileIcon());

            return new StreamImageSource
            {
                Stream = _ => Task.FromResult<Stream>(new MemoryStream(data, writable: false))
            };
        }

        private static byte[] SnapshotBytes(Stream stream)
        {
            try
            {
                // MemoryStream.ToArray is valid even after the stream has been closed
                // by a previous image decoding
                if (stream is MemoryStream memoryStream)
                    return memoryStream.ToArray();

                if (!stream.CanRead || !stream.CanSeek)
                    return [];

                var savedPosition = stream.Position;
                stream.Position = 0L;

                var data = new byte[stream.Length];
                stream.ReadExactly(data);
                stream.TrySetPositionOrAdvance(savedPosition);

                return data;
            }
            catch (Exception)
            {
                return [];
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
