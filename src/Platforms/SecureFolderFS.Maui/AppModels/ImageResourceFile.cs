using IImage = SecureFolderFS.Shared.ComponentModel.IImage;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="IImage"/>
    internal sealed class ImageResourceFile : IImage
    {
        public string Name { get; }

        public bool IsResource { get; }

        public ImageResourceFile(string name, bool isResource = false)
        {
            Name = name;
            IsResource = isResource;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
