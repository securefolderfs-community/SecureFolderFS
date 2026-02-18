using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Components
{
    [Bindable(true)]
    public sealed partial class FileSystemItemViewModel(IFileSystemInfo fileSystem)
        : PickerOptionViewModel(fileSystem.Id, fileSystem.Name)
    {
        /// <summary>
        /// Gets or sets the icon image associated with this instance.
        /// </summary>
        [ObservableProperty] private IImage? _Icon;

        /// <summary>
        /// Gets or sets a value indicating whether this item should be interpreted as the default one.
        /// </summary>
        [ObservableProperty] private bool _IsDefault;

        /// <summary>
        /// Gets the file system identifier abstraction.
        /// </summary>
        public IFileSystemInfo FileSystem { get; } = fileSystem;
    }
}