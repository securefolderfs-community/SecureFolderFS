using System.IO;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Models
{
    /// <inheritdoc cref="IImageStream"/>
    public sealed class StreamImageModel : IImageStream
    {
        /// <inheritdoc/>
        public Stream Inner { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamImageModel"/> class.
        /// </summary>
        /// <param name="inner">The stream containing image data.</param>
        public StreamImageModel(Stream inner)
        {
            Inner = inner;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Inner.Dispose();
        }
    }
}


