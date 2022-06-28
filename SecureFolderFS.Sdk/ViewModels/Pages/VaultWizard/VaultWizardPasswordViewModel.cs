using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.VaultCreator.Routine;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardPasswordViewModel : BaseVaultWizardPageViewModel
    {
        private readonly IFolder _vaultFolder;
        private readonly IVaultCreationRoutineStep7 _step7;

        public Func<DisposablePassword>? InitializeWithPassword { get; set; }

        public VaultWizardPasswordViewModel(IFolder vaultFolder, IVaultCreationRoutineStep7 step7, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            _vaultFolder = vaultFolder;
            _step7 = step7;
        }

        public override Task PrimaryButtonClick(IEventDispatchFlag? flag)
        {
            flag?.NoForwarding();

            var step9 = _step7
                .InitializeKeystoreData(InitializeWithPassword!())
                .ContinueKeystoreFileInitialization();

            Messenger.Send(new NavigationRequestedMessage(new VaultWizardEncryptionViewModel(_vaultFolder, step9, Messenger, DialogViewModel)));
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _step7.Dispose();
        }
    }
}
