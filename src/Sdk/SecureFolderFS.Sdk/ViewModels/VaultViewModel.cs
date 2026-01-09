using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels
{
    [Inject<IVaultService>, Inject<IVaultFileSystemService>, Inject<IVaultPersistenceService>]
    [Bindable(true)]
    public sealed partial class VaultViewModel : ObservableObject, IViewable, IDisposable, IRecipient<VaultLockedMessage>
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _CanRename;
        [ObservableProperty] private bool _IsUnlocked;
        [ObservableProperty] private DateTime? _LastAccessDate;

        /// <summary>
        /// Gets the model representing the current vault.
        /// </summary>
        public IVaultModel VaultModel { get; }

        public VaultViewModel(IVaultModel vaultModel)
        {
            ServiceProvider = DI.Default;
            Title = vaultModel.DataModel.DisplayName ?? vaultModel.VaultFolder?.Name;
            VaultModel = vaultModel;
            CanRename = !vaultModel.IsRemote || vaultModel.VaultFolder is not null;
            LastAccessDate = vaultModel.DataModel.LastAccessDate;
            vaultModel.StateChanged += VaultModel_StateChanged;

            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
        }

        [RelayCommand]
        public async Task SetNameAsync(string? newName, CancellationToken cancellationToken)
        {
            if (newName is null)
                return;

            if (string.IsNullOrEmpty(newName))
                newName = Path.GetFileName(VaultModel.VaultFolder?.Id);

            if (newName is null)
                return;

            VaultModel.DataModel.DisplayName = newName;
            Title = newName;

            await VaultModel.TrySaveAsync(cancellationToken);
        }

        [RelayCommand]
        public async Task SetLastAccessDateAsync(DateTime? newDate, CancellationToken cancellationToken)
        {
            if (newDate is null)
                return;

            VaultModel.DataModel.LastAccessDate = newDate;
            LastAccessDate = newDate;

            await VaultModel.TrySaveAsync(cancellationToken);
        }

        public async Task<UnlockedVaultViewModel> UnlockAsync(IDisposable unlockContract, bool isReadOnly)
        {
            if (IsUnlocked)
                throw new InvalidOperationException("The vault is already unlocked.");

            if (VaultModel.VaultFolder is not { } vaultFolder)
                throw new InvalidOperationException("The vault folder is not set.");

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
            await SetLastAccessDateAsync(DateTime.Now, CancellationToken.None);

            // Notify that the vault has been unlocked
            IsUnlocked = true;
            WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(VaultModel));

            return new(vaultFolder, storageRoot, this);
        }

        private void VaultModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is not VaultChangedEventArgs)
                return;

            CanRename = !VaultModel.IsRemote || VaultModel.VaultFolder is not null;
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (VaultModel.Equals(message.VaultModel))
                IsUnlocked = false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
            VaultModel.StateChanged -= VaultModel_StateChanged;
            VaultModel.Dispose();
        }
    }
}
