using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels
{
    [Inject<IVaultFileSystemService>]
    [Bindable(true)]
    public sealed partial class VaultViewModel : ObservableObject
    {
        [ObservableProperty] private string _VaultName;
        [ObservableProperty] private DateTime? _LastAccessDate;

        public IVaultModel VaultModel { get; }

        public VaultViewModel(IVaultModel vaultModel)
        {
            ServiceProvider = DI.Default;
            VaultModel = vaultModel;
            VaultName = vaultModel.VaultName;
            LastAccessDate = vaultModel.LastAccessDate;
        }

        public async Task<UnlockedVaultViewModel> UnlockAsync(IDisposable unlockContract)
        {
            // Get the file system
            var fileSystem = await VaultFileSystemService.GetBestFileSystemAsync();
            var vaultOptions = VaultFileSystemService.GetFileSystemOptions(VaultModel, fileSystem.Id);

            // Create the storage layer
            var contentFolder = await VaultModel.GetContentFolderAsync();
            var storageRoot = await fileSystem.MountAsync(contentFolder, unlockContract, vaultOptions);

            // Update last access date
            await VaultModel.SetLastAccessDateAsync(DateTime.Now);
            LastAccessDate = VaultModel.LastAccessDate;

            // Notify that the vault has been unlocked
            WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(VaultModel));

            return new(storageRoot, this);
        }
    }
}
