using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Vault.Login;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel
    {
        private IKeystoreModel? _keystoreModel;

        [ObservableProperty]
        private string? _VaultName;

        [ObservableProperty]
        private ObservableObject? _LoginOptionViewModel;

        public VaultLoginPageViewModel(IVaultModel vaultModel)
            : base(vaultModel, new WeakReferenceMessenger())
        {
            VaultName = vaultModel.VaultName;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var is2faEnabled = false; // TODO: Just for testing, implement the real code later
            if (is2faEnabled || await VaultModel.Folder.GetFileAsync(Core.Constants.VAULT_KEYSTORE_FILENAME) is not IFile keystoreFile)
            {
                // TODO: Recognize the option in that view model
                LoginOptionViewModel = new LoginKeystoreSelectionViewModel();
            }
            else
            {
                _keystoreModel ??= new FileKeystoreModel(keystoreFile, JsonToStreamSerializer.Instance);
                var unlockingModel = new VaultUnlockingModel(VaultModel, _keystoreModel);
                await unlockingModel.InitAsync(cancellationToken);
                LoginOptionViewModel = new LoginCredentialsViewModel(Messenger, VaultModel, unlockingModel);
            }
        }
    }
}
