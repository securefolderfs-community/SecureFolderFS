using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel, IRecipient<ChangeLoginOptionMessage>
    {
        private readonly IVaultLoginModel _vaultLoginModel;
        private readonly IAsyncValidator<IFolder> _vaultValidator;

        [ObservableProperty]
        private string? _VaultName;

        [ObservableProperty]
        private BaseLoginStrategyViewModel? _LoginStrategyViewModel;

        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        private IThreadingService ThreadingService { get; } = Ioc.Default.GetRequiredService<IThreadingService>();

        public VaultLoginPageViewModel(IVaultModel vaultModel)
            : base(vaultModel, new WeakReferenceMessenger())
        {
            VaultName = vaultModel.VaultName;
            _vaultValidator = VaultService.GetVaultValidator();
            _vaultLoginModel = new VaultLoginModel(vaultModel);
            _vaultLoginModel.VaultChangedEvent += VaultLoginModel_VaultChangedEvent;
        }

        private async void VaultLoginModel_VaultChangedEvent(object? sender, IResult e)
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
            await _vaultLoginModel.WatchForChangesAsync(cancellationToken);
            await DetermineLoginStrategyAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Receive(ChangeLoginOptionMessage message)
        {
            LoginStrategyViewModel = message.ViewModel;
        }

        // TODO: Move to separate method and add file system watcher for any vault changes.
        private async Task DetermineLoginStrategyAsync(CancellationToken cancellationToken = default)
        {
            var is2faEnabled = false; // TODO: Just for testing, implement the real code later
            if (is2faEnabled || await VaultModel.Folder.TryGetFileAsync(Core.Constants.VAULT_KEYSTORE_FILENAME, cancellationToken) is not IFile keystoreFile)
            {
                // TODO: Recognize the option in that view model
                LoginStrategyViewModel = new LoginKeystoreSelectionViewModel();
                return;
            }

            var validationResult = await _vaultValidator.ValidateAsync(VaultModel.Folder, cancellationToken);
            if (validationResult.Successful)
            {
                var fileSystems = await VaultService.GetFileSystemsAsync(cancellationToken).ToListAsync(cancellationToken);
                var keystoreModel = new FileKeystoreModel(keystoreFile, StreamSerializer.Instance);
                var vaultUnlockingModel = new VaultUnlockingModel(fileSystems[0]);

                LoginStrategyViewModel = new LoginCredentialsViewModel(vaultUnlockingModel, _vaultLoginModel, keystoreModel, Messenger);
            }
            else
                LoginStrategyViewModel = new LoginInvalidVaultViewModel(validationResult.GetMessage("Vault is inaccessible."));
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _vaultLoginModel.VaultChangedEvent -= VaultLoginModel_VaultChangedEvent;
            LoginStrategyViewModel?.Dispose();
        }
    }
}
