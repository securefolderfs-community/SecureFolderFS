using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Banners
{
    [Inject<IVaultFileSystemService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class FileSystemBannerViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty] private ObservableCollection<FileSystemItemViewModel> _FileSystemAdapters;
        [ObservableProperty] private FileSystemItemViewModel? _SelectedItem;
        [ObservableProperty] private InfoBarViewModel _FileSystemInfoBar;

        public FileSystemBannerViewModel()
        {
            ServiceProvider = DI.Default;
            FileSystemInfoBar = new();
            FileSystemAdapters = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in VaultFileSystemService.GetFileSystemsAsync(cancellationToken))
            {
                var fileSystemResult = await item.GetStatusAsync(cancellationToken);
                if (fileSystemResult == FileSystemAvailability.Available)
                    FileSystemAdapters.Add(new(item, item.Name));
            }

            SelectedItem = FileSystemAdapters.FirstOrDefault(x => x.FileSystem.Id == SettingsService.UserSettings.PreferredFileSystemId);
        }

        partial void OnSelectedItemChanged(FileSystemItemViewModel? value)
        {
            if (value is not null)
                SettingsService.UserSettings.PreferredFileSystemId = value.FileSystem.Id;
        }
    }

    public sealed record FileSystemItemViewModel(IFileSystem FileSystem, string Name)
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
