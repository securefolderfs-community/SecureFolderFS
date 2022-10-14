using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel, IRecipient<ChangeLoginOptionMessage>
    {
        [ObservableProperty]
        private string? _VaultName;

        [ObservableProperty]
        private BaseLoginStrategyViewModel? _LoginStrategyViewModel;

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

        private async Task DetermineLoginStrategyAsync(CancellationToken cancellationToken)
        {
            var is2faEnabled = false; // TODO: Just for testing, implement the real code later
            if (is2faEnabled || await VaultModel.Folder.TryGetFileAsync(Core.Constants.VAULT_KEYSTORE_FILENAME, cancellationToken) is not IFile keystoreFile)
            {
                // TODO: Recognize the option in that view model
                LoginStrategyViewModel = new LoginKeystoreSelectionViewModel();
            }
            else
            {
                var vaultService = Ioc.Default.GetRequiredService<IVaultService>();
                var fileSystems = await vaultService.GetFileSystemsAsync(cancellationToken).ToListAsync(cancellationToken);

                var keystoreModel = new FileKeystoreModel(keystoreFile, StreamSerializer.Instance);
                var vaultUnlockingModel = new VaultUnlockingModel(fileSystems[0]);

                LoginStrategyViewModel = new LoginCredentialsViewModel(vaultUnlockingModel, keystoreModel, VaultModel, Messenger);
            }
        }
    }
}
