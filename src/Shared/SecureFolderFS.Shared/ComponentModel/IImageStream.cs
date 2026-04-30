using System.IO;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents an image that can be read from a <see cref="Stream"/>.
    /// </summary>
    public interface IImageStream : IImage, IWrapper<Stream>
    {
    }
}
