using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels
{
    [Inject<IVaultService>, Inject<IVaultFileSystemService>, Inject<IVaultPersistenceService>]
    [Bindable(true)]
    public sealed partial class VaultViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private DateTime? _LastAccessDate;

        [Obsolete]
        public IVaultModel VaultModel { get; }

        public VaultViewModel(IVaultModel vaultModel)
        {
            ServiceProvider = DI.Default;
            Title = vaultModel.VaultName;
            VaultModel = vaultModel;
            LastAccessDate = vaultModel.LastAccessDate;
        }

        [RelayCommand]
        public async Task SetNameAsync(string? newName, CancellationToken cancellationToken)
        {
            if (newName is null)
                return;

            var dataModel = VaultPersistenceService.VaultConfigurations.PersistedVaults?.FirstOrDefault(x => x.PersistableId == VaultModel.Folder.Id);
            if (dataModel is null)
                return;

            if (string.IsNullOrEmpty(newName))
                newName = Path.GetFileName(VaultModel.Folder.Id);

            dataModel.DisplayName = newName;
            Title = newName;

            await VaultPersistenceService.VaultConfigurations.TrySaveAsync(cancellationToken);
        }

        public async Task<UnlockedVaultViewModel> UnlockAsync(IFolder vaultFolder, IDisposable unlockContract, bool isReadOnly)
        {
            // Get the file system
            var fileSystem = await VaultFileSystemService.GetBestFileSystemAsync();

            // Format volume name
            var volumeName = FormattingHelpers.SanitizeVolumeName(Title ?? vaultFolder.Name, vaultFolder.Name);

            // Configure options
            var options = new Dictionary<string, object>()
            {
                { nameof(VirtualFileSystemOptions.IsReadOnly), isReadOnly },
                { nameof(VirtualFileSystemOptions.VolumeName), volumeName }
            };

            var contentFolder = await VaultHelpers.GetOrCreateContentFolderAsync(vaultFolder, CancellationToken.None);
            _ = contentFolder ?? throw new InvalidOperationException("Could not retrieve the content folder.");

            // Create the storage layer
            var storageRoot = await fileSystem.MountAsync(contentFolder, unlockContract, options);

            // Update last access date
            await VaultModel.SetLastAccessDateAsync(DateTime.Now);
            LastAccessDate = VaultModel.LastAccessDate;

            // Notify that the vault has been unlocked
            WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(VaultModel));

            return new(vaultFolder, storageRoot, this);
        }
    }
}
