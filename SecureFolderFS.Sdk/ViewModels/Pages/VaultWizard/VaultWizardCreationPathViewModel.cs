using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardCreationPathViewModel : VaultWizardPathSelectionBaseViewModel<IFolder>
    {
        private VaultWizardPasswordViewModel? _nextViewModel;

        public VaultWizardCreationPathViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
        }

        public override Task PrimaryButtonClick(IEventDispatchFlag? flag)
        {
            // Cancel the confirm button
            flag?.NoForwarding();

            // We've already initialized the data
            if (_nextViewModel is not null)
            {
                Messenger.Send(new NavigationRequestedMessage<VaultWizardPasswordViewModel>(_nextViewModel));
                return Task.CompletedTask;
            }

            // Continue with initialization
            var step7 = VaultRoutines.NewVaultCreationRoutine()
                .EstablishRoutine()
                .SetVaultPath(new(SelectedLocation!.Path))
                .AddFileOperations()
                .CreateConfigurationFile()
                .CreateKeystoreFile()
                .CreateContentFolder()
                .AddEncryptionAlgorithmBuilder();

            _nextViewModel = new(SelectedLocation, step7, Messenger, DialogViewModel);


            DialogViewModel.VaultViewModel = new(new(), PathSourceText!);
            NextViewModel = new VaultWizardPasswordViewModel(step7, Messenger, DialogViewModel);
            Messenger.Send(new VaultWizardNavigationRequestedMessage(NextViewModel));

            return Task.CompletedTask;
        }

        public override Task<bool> SetLocation(IFolder storage)
        {
            LocationPath = storage.Path;
            SelectedLocation = storage;
            DialogViewModel.PrimaryButtonEnabled = true;

            return Task.FromResult(true);
        }

        protected override async Task BrowseLocationAsync()
        {
            var folder = await FileExplorerService.PickSingleFolderAsync();
            if (folder is not null)
                await SetLocation(folder);
        }
    }
}
