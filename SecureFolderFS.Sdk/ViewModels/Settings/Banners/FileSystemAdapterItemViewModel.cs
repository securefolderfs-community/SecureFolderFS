using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Settings.Banners
{
    public sealed class FileSystemAdapterItemViewModel : ObservableObject
    {
        public IFileSystemInfoModel FileSystemInfoModel { get; }

        public string Name { get; }

        public FileSystemAdapterItemViewModel(IFileSystemInfoModel fileSystemInfoModel)
        {
            FileSystemInfoModel = fileSystemInfoModel;
            Name = fileSystemInfoModel.Name;
        }
    }
}
