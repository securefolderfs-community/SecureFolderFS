using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.VaultCreator.Routine;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public sealed class ChooseEncryptionPageViewModel : BaseVaultWizardPageViewModel
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

        public ChooseEncryptionPageViewModel(IVaultCreationRoutineStep9 step9, IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            this._step9 = step9;
            base.CanGoBack = false;

            DialogViewModel.PrimaryButtonEnabled = true;
            DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<IHandledFlag?>(PrimaryButtonClick);
        }

        private void PrimaryButtonClick(IHandledFlag? e)
        {
            e?.Handle();

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

            Messenger.Send(new VaultWizardNavigationRequestedMessage(new VaultWizardFinishPageViewModel(Messenger, DialogViewModel)));
        }

        public override void Dispose()
        {
            _step9.Dispose();
        }
    }
}
