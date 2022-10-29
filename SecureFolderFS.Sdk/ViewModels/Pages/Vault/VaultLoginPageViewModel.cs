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
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel, IRecipient<ChangeLoginOptionMessage>
    {
        [ObservableProperty]
        private string? _VaultName;

        [ObservableProperty]
        private BaseLoginStrategyViewModel? _LoginStrategyViewModel;

        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        public VaultLoginPageViewModel(IVaultModel vaultModel)
            : base(vaultModel, new WeakReferenceMessenger())
        {
            VaultName = vaultModel.VaultName;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await DetermineLoginStrategyAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Receive(ChangeLoginOptionMessage message)
        {
            LoginStrategyViewModel = message.ViewModel;
        }

        // TODO: Move to separate method and add file system watcher for any vault changes.
        private async Task DetermineLoginStrategyAsync(CancellationToken cancellationToken)
        {
            var is2faEnabled = false; // TODO: Just for testing, implement the real code later
            if (is2faEnabled || await VaultModel.Folder.TryGetFileAsync(Core.Constants.VAULT_KEYSTORE_FILENAME, cancellationToken) is not IFile keystoreFile)
            {
                // TODO: Recognize the option in that view model
                LoginStrategyViewModel = new LoginKeystoreSelectionViewModel();
                return;
            }

            var vaultValidator = VaultService.GetVaultValidator();
            var validationResult = await vaultValidator.ValidateAsync(VaultModel.Folder, cancellationToken);

            if (validationResult.Successful)
            {
                var vaultService = Ioc.Default.GetRequiredService<IVaultService>();
                var fileSystems = await vaultService.GetFileSystemsAsync(cancellationToken).ToListAsync(cancellationToken);

                var keystoreModel = new FileKeystoreModel(keystoreFile, StreamSerializer.Instance);
                var vaultUnlockingModel = new VaultUnlockingModel(fileSystems[0]);

                LoginStrategyViewModel = new LoginCredentialsViewModel(vaultUnlockingModel, keystoreModel, VaultModel, Messenger);
            }
            else
            {
                // TODO: Improve error message
                LoginStrategyViewModel = new LoginInvalidVaultViewModel("Vault is inaccessible.");
            }
        }
    }
}
