using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Uno.AppModels;
using SecureFolderFS.Uno.Helpers;

#if WINDOWS
using System.Runtime.InteropServices;
using Vanara.PInvoke;
#endif

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal sealed class UnoMediaService : IMediaService
    {
        /// <inheritdoc/>
        public Task<IImage> GetImageFromResourceAsync(string resourceName, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IImage>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public Task<IImage> GetImageFromUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IImage>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public async Task<IImage> ReadImageFileAsync(IFile file, CancellationToken cancellationToken)
        {
            await using var stream = await file.OpenStreamAsync(FileAccess.Read, cancellationToken);
            using var winrtStream = stream.AsRandomAccessStream();

            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(winrtStream).AsTask(cancellationToken);

            return new ImageBitmap(bitmapImage, null);

            // TODO: Check if it works
            var classification = FileTypeHelper.GetClassification(file);
            return await ImagingHelpers.GetBitmapFromStreamAsync(winrtStream, classification.MimeType, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IDisposable> StreamVideoAsync(IFile file, CancellationToken cancellationToken)
        {
            return Task.FromException<IDisposable>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public Task<IImageStream> GenerateThumbnailAsync(IFile file, TypeHint typeHint = default, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IImageStream>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public Task<IDisposable> StreamPdfSourceAsync(IFile file, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IDisposable>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public async Task<bool> TrySetFolderIconAsync(IModifiableFolder folder, Stream imageStream, CancellationToken cancellationToken = default)
        {
#if WINDOWS
            try
            {
                // Create icon file
                var iconFile = await folder.CreateFileAsync(Sdk.Constants.Vault.VAULT_ICON_FILENAME_ICO, true, cancellationToken);
                await using var iconStream = await iconFile.OpenReadWriteAsync(cancellationToken);
                await ImageToIconAsync(imageStream, iconStream, 100, true, cancellationToken);

                // Set desktop.ini data
                var desktopIniFile = await folder.CreateFileAsync("desktop.ini", true, cancellationToken);
                var text = string.Format(UI.Constants.FileData.DESKTOP_INI_ICON_CONFIGURATION, Sdk.Constants.Vault.VAULT_ICON_FILENAME_ICO, "Encrypted vault data folder");
                await desktopIniFile.WriteTextAsync(text, cancellationToken);

                // Notify Shell of the update
                File.SetAttributes(desktopIniFile.Id, FileAttributes.Hidden | FileAttributes.System);
                var folderSettings = new Shell32.SHFOLDERCUSTOMSETTINGS()
                {
                    cchIconFile = 0,
                    pszIconFile = iconFile.Name,
                    dwMask = Shell32.FOLDERCUSTOMSETTINGSMASK.FCSM_ICONFILE,
                    dwSize = (uint)Marshal.SizeOf<Shell32.SHFOLDERCUSTOMSETTINGS>()
                };
                Shell32.SHGetSetFolderCustomSettings(ref folderSettings, folder.Id, Shell32.FCS.FCS_FORCEWRITE);
                Shell32.SHChangeNotify(Shell32.SHCNE.SHCNE_UPDATEITEM, Shell32.SHCNF.SHCNF_PATHW, folder.Id);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
#else 
            await Task.CompletedTask;
            return false;
#endif
        }

        private async Task ImageToIconAsync(Stream imageStream, Stream destinationStream, int size, bool preserveAspectRatio, CancellationToken cancellationToken)
        {
            var inputBitmap = Image.FromStream(imageStream);
            int width;
            int height;

            if (preserveAspectRatio)
            {
                width = size;
                height = inputBitmap.Height / inputBitmap.Width * size;
            }
            else
                width = height = size;

            var newBitmap = new Bitmap(inputBitmap, new(width, height));
            await using var resizedBitmapStream = new MemoryStream();
            newBitmap.Save(resizedBitmapStream, ImageFormat.Png);

            await using var writer = new BinaryWriter(destinationStream);

            // Header
            writer.Write((short)0);     // 0 : reserved
            writer.Write((short)1);     // 2 : 1=ico, 2=cur
            writer.Write((short)1);     // 4 : number of images

            // Image Entry
            writer.Write((byte)Math.Min(width, byte.MaxValue));     // 0 : width
            writer.Write((byte)Math.Min(height, byte.MaxValue));    // 1 : height
            writer.Write((byte)0);                                  // 2 : number of colors
            writer.Write((byte)0);                                  // 3 : reserved
            writer.Write((short)0);                                 // 4 : number of color planes
            writer.Write((short)32);                                // 6 : bits per pixel
            writer.Write((int)resizedBitmapStream.Length);          // 8 : size of image data
            writer.Write((int)(6 + 16));                            // 12 : offset of image data

            writer.Write(resizedBitmapStream.ToArray());
            writer.Flush();
        }
    }
}
