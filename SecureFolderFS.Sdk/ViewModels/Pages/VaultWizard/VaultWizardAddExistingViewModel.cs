using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardAddExistingViewModel : BaseVaultWizardPageViewModel // TODO: Refactor
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

        public VaultWizardAddExistingViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.PrimaryButtonEnabled = false;
            BrowseForFolderCommand = new AsyncRelayCommand(BrowseForFolder);
        }

        public override Task PrimaryButtonClick(IEventDispatchFlag? flag)
        {
            flag?.NoForwarding();
            
            DialogViewModel.VaultViewModel = new(new(), Path.GetDirectoryName(PathSourceText!)!);
            Messenger.Send(new VaultWizardNavigationRequestedMessage(new VaultWizardSummaryViewModel(Messenger, DialogViewModel)));

            return Task.CompletedTask;
        }

        private async Task BrowseForFolder()
        {
            var file = await FileExplorerService.PickSingleFileAsync(new List<string>() { Path.GetExtension(SecureFolderFS.Core.Constants.VAULT_CONFIGURATION_FILENAME) });
            PathSourceText = file?.Path ?? PathSourceText;
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
            
            return false;
        }
    }
}
