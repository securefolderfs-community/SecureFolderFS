using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    public interface IMediaService
    {
        /// <summary>
        /// Reads an image file and returns a new instance of <see cref="IImage"/>
        /// </summary>
        /// <param name="file">The <see cref="IFile"/> to read.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the image read from file.</returns>
        Task<IImage> ReadImageFileAsync(IFile file, CancellationToken cancellationToken = default);

        Task<IImageStream?> GenerateThumbnailAsync(IFile file, CancellationToken cancellationToken = default);

        Task<IDisposable> StreamVideoAsync(IFile file, CancellationToken cancellationToken = default);

        Task<bool> TrySetFolderIconAsync(IModifiableFolder folder, Stream imageStream, CancellationToken cancellationToken = default);
    }
}
