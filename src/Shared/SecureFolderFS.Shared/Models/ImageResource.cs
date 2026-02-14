using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Models
{
    /// <inheritdoc cref="IImage"/>
    public class ImageResource(string name, bool isResource = false) : IImage
    {
        /// <summary>
        /// Gets the name of the resource.
        /// </summary>
        public virtual string Name { get; } = name;

        /// <summary>
        /// Gets whether the resource is a custom resource.
        /// </summary>
        public virtual bool IsResource { get; } = isResource;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}