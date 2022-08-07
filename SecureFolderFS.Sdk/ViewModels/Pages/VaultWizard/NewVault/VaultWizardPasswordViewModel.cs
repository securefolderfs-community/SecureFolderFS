using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Core.VaultCreator.Routine;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardPasswordViewModel : BaseVaultWizardPageViewModel, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly IVaultCreationRoutineStep7 _step7;

        public Func<IPassword>? InitializeWithPassword { get; set; }

        /// <inheritdoc cref="DialogViewModel.PrimaryButtonEnabled"/>
        public bool PrimaryButtonEnabled
        {
            get => DialogViewModel.PrimaryButtonEnabled;
            set => DialogViewModel.PrimaryButtonEnabled = value;
        }

        public VaultWizardPasswordViewModel(IFolder vaultFolder, IVaultCreationRoutineStep7 step7, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {

            _vaultFolder = vaultFolder;
            _step7 = step7;

            // Always false since passwords are not preserved
            DialogViewModel.PrimaryButtonEnabled = false;
        }

        /// <inheritdoc/>
        public override Task PrimaryButtonClickAsync(IEventDispatchFlag? flag)
        {
            flag?.NoForwarding();

            var step9 = _step7
                .InitializeKeystoreData(InitializeWithPassword!())
                .ContinueKeystoreFileInitialization();

            Messenger.Send(new NavigationRequestedMessage(new VaultWizardEncryptionViewModel(_vaultFolder, step9, Messenger, DialogViewModel)));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _step7.Dispose();
        }
    }
}
