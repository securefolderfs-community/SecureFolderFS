using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.VaultCreator.Routine;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardEncryptionViewModel : BaseVaultWizardPageViewModel
    {
        private readonly IVaultCreationRoutineStep9 _step9;

        private int _SelectedDataEncryptionIndex;
        public int SelectedDataEncryptionIndex
        {
            get => _SelectedDataEncryptionIndex;
            set => SetProperty(ref _SelectedDataEncryptionIndex, value);
        }

        private int _SelectedFileNameEncryptionIndex;
        public int SelectedFileNameEncryptionIndex
        {
            get => _SelectedFileNameEncryptionIndex;
            set => SetProperty(ref _SelectedFileNameEncryptionIndex, value);
        }

        public VaultWizardEncryptionViewModel(IVaultCreationRoutineStep9 step9, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            _step9 = step9;
            CanGoBack = false;

            DialogViewModel.PrimaryButtonEnabled = true;
            DialogViewModel.SecondaryButtonText = null; // Don't show the option to cancel the dialog
        }

        public override Task PrimaryButtonClick(IEventDispatchFlag? flag)
        {
            flag?.NoForwarding();

            var contentCipher = SelectedDataEncryptionIndex switch
            {
                0 => ContentCipherScheme.XChaCha20_Poly1305,
                1 => ContentCipherScheme.AES_GCM,
                2 => ContentCipherScheme.AES_CTR_HMAC,
                _ => throw new ArgumentOutOfRangeException(nameof(_SelectedDataEncryptionIndex))
            };
            var nameCipher = SelectedFileNameEncryptionIndex switch
            {
                0 => FileNameCipherScheme.AES_SIV,
                1 => FileNameCipherScheme.None,
                _ => throw new ArgumentOutOfRangeException(nameof(SelectedFileNameEncryptionIndex))
            };

            _step9.SetContentCipherScheme(contentCipher)
                .SetFileNameCipherScheme(nameCipher)
                .ContinueConfigurationFileInitialization()
                .Finalize()
                .Deploy();

            Messenger.Send(new VaultWizardNavigationRequestedMessage(new VaultWizardSummaryViewModel(Messenger, DialogViewModel)));

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _step9.Dispose();
        }
    }
}
