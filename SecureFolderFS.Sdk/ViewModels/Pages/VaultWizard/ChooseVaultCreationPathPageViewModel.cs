using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class ChooseVaultCreationPathPageViewModel : BaseVaultWizardPageViewModel // TODO: Refactor
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        private string? _PathSourceText;
        public string? PathSourceText
        {
            get => _PathSourceText;
            set
            {
                if (SetProperty(ref _PathSourceText, value))
                    DialogViewModel.PrimaryButtonEnabled = CheckAvailability(value);
            }
        }

        public IAsyncRelayCommand BrowseForFolderCommand { get; }

        public ChooseVaultCreationPathPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.PrimaryButtonEnabled = false;
            BrowseForFolderCommand = new AsyncRelayCommand(BrowseForFolder);
        }

        public override Task PrimaryButtonClick(IEventDispatchFlag? flag)
        {
            // Cancel the confirm button
            flag?.NoForwarding();

            // We've already got the next view model
            if (NextViewModel is not null)
            {
                Messenger.Send(new VaultWizardNavigationRequestedMessage(NextViewModel));
                return Task.CompletedTask;
            }

            // Continue with initialization
            var step7 = VaultRoutines.NewVaultCreationRoutine()
                .EstablishRoutine()
                .SetVaultPath(new(PathSourceText))
                .AddFileOperations()
                .CreateConfigurationFile()
                .CreateKeystoreFile()
                .CreateContentFolder()
                .AddEncryptionAlgorithmBuilder();

            DialogViewModel.VaultViewModel = new(new(), PathSourceText!);
            NextViewModel = new SetPasswordPageViewModel(step7, Messenger, DialogViewModel);
            Messenger.Send(new VaultWizardNavigationRequestedMessage(NextViewModel));

            return Task.CompletedTask;
        }

        public override void ReturnToViewModel()
        {
            DialogViewModel.PrimaryButtonEnabled = CheckAvailability(PathSourceText);
        }

        private async Task BrowseForFolder()
        {
            var path = await FileExplorerService.PickSingleFolderAsync();
            if (!string.IsNullOrEmpty(path))
            {
                PathSourceText = path;
            }
        }

        private static bool CheckAvailability(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return Directory.Exists(path);
        }
    }
}
