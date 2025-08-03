using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
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
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels
{
    [Inject<IVaultService>]
    [Inject<IVaultFileSystemService>]
    [Bindable(true)]
    public sealed partial class VaultViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private DateTime? _LastAccessDate;

        public IVaultModel VaultModel { get; }

        public VaultViewModel(IVaultModel vaultModel)
        {
            ServiceProvider = DI.Default;
            Title = vaultModel.VaultName;
            VaultModel = vaultModel;
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
                { nameof(VirtualFileSystemOptions.IsReadOnly), isReadOnly },
                { nameof(VirtualFileSystemOptions.VolumeName), volumeName }
            };

            var contentFolder = await GetOrCreateContentFolder();
            _ = contentFolder ?? throw new InvalidOperationException("Could not retrieve the content folder.");

            // Create the storage layer
            var storageRoot = await fileSystem.MountAsync(contentFolder, unlockContract, options);

            // Update last access date
            await VaultModel.SetLastAccessDateAsync(DateTime.Now);
            LastAccessDate = VaultModel.LastAccessDate;

            // Notify that the vault has been unlocked
            WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(VaultModel));

            return new(storageRoot, this);
        }

        private async Task<IFolder?> GetOrCreateContentFolder()
        {
            var contentFolder = await SafetyHelpers.NoThrowAsync(async () => await VaultModel.GetContentFolderAsync());
            if (VaultModel.Folder is not IModifiableFolder modifiableFolder)
                return contentFolder;

            return contentFolder ?? await modifiableFolder.CreateFolderAsync(VaultService.ContentFolderName);
        }
    }
}
