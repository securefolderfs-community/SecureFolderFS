using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public sealed class AddExistingVaultPageViewModel : BaseVaultWizardPageViewModel
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
                    DialogViewModel.PrimaryButtonEnabled = CheckAvailability(value);
                }
            }
        }

        public IAsyncRelayCommand BrowseForFolderCommand { get; }

        public AddExistingVaultPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<IHandledFlag?>(PrimaryButtonClick);
            BrowseForFolderCommand = new AsyncRelayCommand(BrowseForFolder);
        }

        private void PrimaryButtonClick(IHandledFlag? e)
        {
            e?.Handle();
            DialogViewModel.VaultViewModel = new(new(), Path.GetDirectoryName(PathSourceText!)!);
            
            Messenger.Send(new VaultWizardNavigationRequestedMessage(new VaultWizardFinishPageViewModel(Messenger, DialogViewModel)));
        }

        private async Task BrowseForFolder()
        {
            var path = await FileExplorerService.PickSingleFileAsync(new List<string>() { Path.GetExtension(SecureFolderFS.Core.Constants.VAULT_CONFIGURATION_FILENAME) });
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

            path = Path.Combine(Path.GetDirectoryName(path)!, SecureFolderFS.Core.Constants.VAULT_CONFIGURATION_FILENAME);
            if (File.Exists(path))
            {
                using var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

                var rawVaultConfiguration = RawVaultConfiguration.Load(fileStream);
                return VaultVersion.IsVersionSupported((VaultVersion)rawVaultConfiguration);
            }
            else
            {
                return false;
            }
        }
    }
}
