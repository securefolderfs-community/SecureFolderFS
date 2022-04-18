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

        private SetPasswordPageViewModel? _nextViewModel;

        private string? _PathSourceText;
        public string? PathSourceText
        {
            get => _PathSourceText;
            set
            {
                if (SetProperty(ref _PathSourceText, value))
                {
                    DialogViewModel.PrimaryButtonEnabled = CheckAvailability(value);
                }
            }
        }

        public IAsyncRelayCommand BrowseForFolderCommand { get; }

        public ChooseVaultCreationPathPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.PrimaryButtonEnabled = false;

            DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<IHandledFlag?>(PrimaryButtonClick);
            BrowseForFolderCommand = new AsyncRelayCommand(BrowseForFolder);
        }

        public override void ReattachCommands()
        {
            DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<IHandledFlag?>(e =>
            {
                e?.Handle();

                Messenger.Send(new VaultWizardNavigationRequestedMessage(_nextViewModel!));
            });
            DialogViewModel.PrimaryButtonEnabled = CheckAvailability(_PathSourceText);
        }

        private void PrimaryButtonClick(IHandledFlag? e)
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

            DialogViewModel.VaultViewModel = new(new(), PathSourceText!);
            _nextViewModel = new(step7, Messenger, DialogViewModel);

            // Navigate
            Messenger.Send(new VaultWizardNavigationRequestedMessage(_nextViewModel));
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
    }
}
