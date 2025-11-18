using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Provides functionality for handling media files such as images, videos, and document thumbnails.
    /// This interface defines methods to read, stream, or generate content for media files.
    /// </summary>
    public interface IMediaService
    {
        /// <summary>
        /// Downloads an image from the specified URL and returns a new instance of <see cref="IImage"/>
        /// </summary>
        /// <param name="url">The URL from which to download the image.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the image returned from the URL.</returns>
        Task<IImage> GetImageFromUrlAsync(string url, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads an image file and returns a new instance of <see cref="IImage"/>
        /// </summary>
        /// <param name="file">The <see cref="IFile"/> to read.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the image read from file.</returns>
        Task<IImage> ReadImageFileAsync(IFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams a video file, providing a disposable resource for the video stream.
        /// </summary>
        /// <param name="file">The <see cref="IFile"/> representing the video to stream.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a disposable resource that manages the video stream.</returns>
        Task<IDisposable> StreamVideoAsync(IFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a thumbnail for the given file based on its type and returns an image stream.
        /// </summary>
        /// <param name="file">The <see cref="IFile"/> to generate the thumbnail for.</param>
        /// <param name="typeHint">A hint about the type of the file, represented as a <see cref="TypeHint"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the generated thumbnail as an <see cref="IImageStream"/>.</returns>
        Task<IImageStream> GenerateThumbnailAsync(IFile file, TypeHint typeHint = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams a PDF source from the provided file.
        /// </summary>
        /// <param name="file">The <see cref="IFile"/> representing the PDF to be streamed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a streamable PDF source wrapped in a disposable object.</returns>
        Task<IDisposable> StreamPdfSourceAsync(IFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to set a custom folder icon using the provided image stream.
        /// </summary>
        /// <param name="folder">The <see cref="IModifiableFolder"/> in which the folder icon is to be set.</param>
        /// <param name="imageStream">The <see cref="Stream"/> containing the image data for the icon.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The value of the task is a boolean indicating whether the folder icon was successfully set.</returns>
        Task<bool> TrySetFolderIconAsync(IModifiableFolder folder, Stream imageStream, CancellationToken cancellationToken = default);
    }
}
