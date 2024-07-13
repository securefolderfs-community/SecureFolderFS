using Foundation;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using UIKit;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage
{
    /// <inheritdoc cref="IStorableChild"/>
    internal abstract class IOSStorable : IStorableChild, IWrapper<NSUrl>
    {
        protected readonly IOSFolder? parent;
        protected readonly NSUrl permissionRoot;

        /// <inheritdoc/>
        public NSUrl Inner { get; }

        /// <inheritdoc/>
        public virtual string Id { get; }

        /// <inheritdoc/>
        public virtual string Name { get; }

        protected IOSStorable(NSUrl url, IOSFolder? parent = null, NSUrl? permissionRoot = null)
        {
            Inner = url;
            this.parent = parent;
            this.permissionRoot = permissionRoot ?? url;

            GetImmediateProperties(url, out var id, out var name);
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder?>(parent);
        }

        protected static void GetImmediateProperties(NSUrl url, out string? id, out string? name)
        {
            using var document = new UIDocument(url);
            id = document.FileUrl?.Path ?? url.FilePathUrl?.Path;
            name = !string.IsNullOrEmpty(document.LocalizedName) 
                ? document.LocalizedName : (System.IO.Path.GetFileName(id) ?? url.FilePathUrl?.LastPathComponent);
        }

        protected static IStorableChild NewStorage(NSUrl url, IOSFolder? parent = null, NSUrl? permissionRoot = null)
        {
            return url.HasDirectoryPath
                ? new IOSFolder(url, parent, permissionRoot)
                : new IOSFile(url, parent, permissionRoot);
        }
    }
}
