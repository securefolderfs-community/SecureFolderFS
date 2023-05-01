using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault
{
    public sealed class PasswordWizardViewModel : BaseWizardPageViewModel
    {
        private readonly IVaultCreationModel _vaultCreationModel;

        public Func<IPassword?>? InitializeWithPassword { get; set; }

        /// <inheritdoc cref="DialogViewModel.PrimaryButtonEnabled"/>
        public bool PrimaryButtonEnabled
        {
            get => DialogViewModel.PrimaryButtonEnabled;
            set => DialogViewModel.PrimaryButtonEnabled = value;
        }

        public PasswordWizardViewModel(IVaultCreationModel vaultCreationModel, VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            _vaultCreationModel = vaultCreationModel;

            // Always false since passwords are not preserved
            DialogViewModel.PrimaryButtonEnabled = false;
        }

        /// <inheritdoc/>
        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();

            var password = InitializeWithPassword?.Invoke();
            if (password is null)
                return;

            if (!await _vaultCreationModel.SetPasswordAsync(password, cancellationToken))
                return; // TODO: Report issue

            await NavigationService.TryNavigateAsync(() => new EncryptionWizardViewModel(_vaultCreationModel, DialogViewModel));
        }
    }
}
