using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.Core.Routines;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public sealed class ChooseVaultCreationPathPageViewModel : BaseVaultWizardPageViewModel
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        private string? _PathSourceText;
        public string? PathSourceText
        {
            get => _PathSourceText;
            set
            {
                if (SetProperty(ref _PathSourceText, value))
                {
                    DialogViewModel.IsPrimaryButtonEnabled = CheckAvailability(value);
                }
            }
        }

        public IAsyncRelayCommand BrowseForFolderCommand { get; }

        public ChooseVaultCreationPathPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.IsPrimaryButtonEnabled = false;

            DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<HandledCallback?>(PrimaryButtonClick);
            BrowseForFolderCommand = new AsyncRelayCommand(BrowseForFolder);
        }

        private void PrimaryButtonClick(HandledCallback? e)
        {
            // Cancel the confirm button
            e?.Handle();

            var step7 = VaultRoutines.NewVaultCreationRoutine()
                .EstablishRoutine()
                .SetVaultPath(new(PathSourceText))
                .AddFileOperations()
                .CreateConfigurationFile()
                .CreateKeystoreFile()
                .CreateContentFolder()
                .AddEncryptionAlgorithmBuilder();

            // Navigate
            Messenger.Send(new VaultWizardNavigationRequestedMessage(new SetPasswordPageViewModel(step7, Messenger, DialogViewModel)));
        }

        private async Task BrowseForFolder()
        {
            var path = await FileExplorerService.PickSingleFolderAsync();
            if (!string.IsNullOrEmpty(path))
            {
                PathSourceText = path;
            }
        }

        private bool CheckAvailability(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return Directory.Exists(path);
        }

        public override void UpdateViewModelOnReturn()
        {
            // TODO
            //DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<HandledCallback?>(step7 =>
            //    Messenger.Send(new VaultWizardNavigationRequestedMessage(new SetPasswordPageViewModel(step7, Messenger, DialogViewModel))));
        }
    }
}
