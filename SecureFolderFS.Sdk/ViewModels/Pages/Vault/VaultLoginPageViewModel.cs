using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel, IRecipient<VaultUnlockedMessage>, IRecipient<ChangeLoginOptionMessage>
    {
        private readonly IVaultWatcherModel _vaultWatcherModel;
        private readonly IAsyncValidator<IFolder> _vaultValidator;

        [ObservableProperty]
        private string? _VaultName;

        [ObservableProperty]
        private BaseLoginStrategyViewModel? _LoginStrategyViewModel;

        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        private IThreadingService ThreadingService { get; } = Ioc.Default.GetRequiredService<IThreadingService>();

        public VaultLoginPageViewModel(VaultViewModel vaultViewModel)
            : base(vaultViewModel, new WeakReferenceMessenger())
        {
            VaultName = vaultViewModel.VaultModel.VaultName;
            _vaultValidator = VaultService.GetVaultValidator();
            _vaultWatcherModel = new VaultWatcherModel(vaultViewModel.VaultModel);
            _vaultWatcherModel.VaultChangedEvent += VaultWatcherModel_VaultChangedEvent;

            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
        }

        private async void VaultWatcherModel_VaultChangedEvent(object? sender, IResult e)
        {
            if (!e.Successful)
            {
                await ThreadingService.ExecuteOnUiThreadAsync();
                await DetermineLoginStrategyAsync();
            }
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await _vaultWatcherModel.WatchForChangesAsync(cancellationToken);
            await DetermineLoginStrategyAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Receive(ChangeLoginOptionMessage message)
        {
            LoginStrategyViewModel = message.ViewModel;
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            // Free resources that are no longer being used for login
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
                Dispose();
        }

        // TODO: Move to separate method and add file system watcher for any vault changes.
        private async Task DetermineLoginStrategyAsync(CancellationToken cancellationToken = default)
        {
            var validationResult = await _vaultValidator.ValidateAsync(VaultViewModel.VaultModel.Folder, cancellationToken);
            // TODO: Use validationResult for 2fa detection as well

            var is2faEnabled = false; // TODO: Just for testing, implement the real code later
            if (is2faEnabled || await VaultViewModel.VaultModel.Folder.TryGetFileAsync(Core.Constants.VAULT_KEYSTORE_FILENAME, cancellationToken) is not IFile keystoreFile)
            {
                // TODO: Recognize the option in that view model
                LoginStrategyViewModel = new LoginKeystoreSelectionViewModel();
                return;
            }

            if (validationResult.Successful)
            {
                var fileSystems = await VaultService.GetFileSystemsAsync(cancellationToken).ToListAsync(cancellationToken);
                var keystoreModel = new FileKeystoreModel(keystoreFile, StreamSerializer.Instance);

                var dokanyFileSystem = fileSystems[0];
                var webDavFileSystem  = fileSystems[1];
                _ = dokanyFileSystem;
                _ = webDavFileSystem;

                var vaultUnlockingModel = new VaultUnlockingModel(webDavFileSystem);
                LoginStrategyViewModel = new LoginCredentialsViewModel(VaultViewModel, vaultUnlockingModel, _vaultWatcherModel, keystoreModel, Messenger);
            }
            else
                LoginStrategyViewModel = new LoginInvalidVaultViewModel(validationResult.GetMessage("Vault is inaccessible."));
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _vaultWatcherModel.VaultChangedEvent -= VaultWatcherModel_VaultChangedEvent;
            _vaultWatcherModel.Dispose();
            LoginStrategyViewModel?.Dispose();
        }
    }
}
