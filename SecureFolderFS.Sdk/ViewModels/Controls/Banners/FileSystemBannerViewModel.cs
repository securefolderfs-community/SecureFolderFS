using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Banners
{
    public sealed class FileSystemBannerViewModel : ObservableObject, IAsyncInitialize
    {
        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public ObservableCollection<FileSystemAdapterItemViewModel> FileSystemAdapters { get; }

        public string PreferredFileSystemId
        {
            get => SettingsService.UserSettings.PreferredFileSystemId;
            set => SettingsService.UserSettings.PreferredFileSystemId = value;
        }

        public FileSystemBannerViewModel()
        {
            FileSystemAdapters = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in VaultService.GetFileSystems())
            {
                var fileSystemResult = await item.GetStatusAsync(cancellationToken);
                if (fileSystemResult is FileSystemResult { CanSupport: true }
                    || (fileSystemResult.Successful && fileSystemResult is not FileSystemResult))
                    FileSystemAdapters.Add(new(item));
            }
        }
    }

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
