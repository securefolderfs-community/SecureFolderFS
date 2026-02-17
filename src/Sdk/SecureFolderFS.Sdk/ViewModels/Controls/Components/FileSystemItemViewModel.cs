using System.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Components
{
    [Bindable(true)]
    public sealed partial class FileSystemItemViewModel(IFileSystem fileSystem)
        : PickerOptionViewModel(fileSystem.Id, fileSystem.Name)
    {
        public IFileSystem FileSystem { get; } = fileSystem;
    }
}