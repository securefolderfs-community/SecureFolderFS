using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Helpers;

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

        public async Task<UnlockedVaultViewModel> UnlockAsync(IDisposable unlockContract, bool isReadOnly)
        {
            // Get the file system
            var fileSystem = await VaultFileSystemService.GetBestFileSystemAsync();

            // Format volume name
            var volumeName = FormattingHelpers.SanitizeVolumeName(VaultModel.VaultName, VaultModel.Folder.Name);

            // Configure options
            var options = new Dictionary<string, object>()
            {
                { nameof(FileSystemOptions.IsReadOnly), isReadOnly },
                { nameof(FileSystemOptions.VolumeName), volumeName }
            };

            // Create the storage layer
            var contentFolder = await VaultModel.GetContentFolderAsync();
            var storageRoot = await fileSystem.MountAsync(contentFolder, unlockContract, options);

            // Update last access date
            await VaultModel.SetLastAccessDateAsync(DateTime.Now);
            LastAccessDate = VaultModel.LastAccessDate;

            // Notify that the vault has been unlocked
            WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(VaultModel));

            return new(storageRoot, this);
        }
    }
}
