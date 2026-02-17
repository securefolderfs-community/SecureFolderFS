using System.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Components
{
    [Bindable(true)]
    public sealed class FileSystemItemViewModel(IFileSystem fileSystem)
        : PickerOptionViewModel(fileSystem.Id, fileSystem.Name)
    {
        public IFileSystem FileSystem { get; } = fileSystem;

        /// <summary>
        /// Gets or sets a value indicating whether this item should be interpreted as the default one.
        /// </summary>
        public bool IsDefault { get; init; }
    }
}