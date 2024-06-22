using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Banners
{
    [Inject<IVaultService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class FileSystemBannerViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty] private ObservableCollection<FileSystemItemViewModel> _FileSystemAdapters;
        [ObservableProperty] private FileSystemItemViewModel? _SelectedItem;

        public FileSystemBannerViewModel()
        {
            ServiceProvider = Ioc.Default;
            FileSystemAdapters = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in VaultService.GetFileSystems())
            {
                var fileSystemResult = await item.GetStatusAsync(cancellationToken);
                if (fileSystemResult is FileSystemResult { CanSupport: true } || (fileSystemResult.Successful && fileSystemResult is not FileSystemResult))
                    FileSystemAdapters.Add(new(item, item.Name));
            }

            SelectedItem = FileSystemAdapters.FirstOrDefault(x => x.FileSystemInfoModel.Id == SettingsService.UserSettings.PreferredFileSystemId);
        }

        partial void OnSelectedItemChanged(FileSystemItemViewModel? value)
        {
            if (value is not null)
                SettingsService.UserSettings.PreferredFileSystemId = value.FileSystemInfoModel.Id;
        }
    }

    public sealed record FileSystemItemViewModel(IFileSystemInfoModel FileSystemInfoModel, string Name)
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
