using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;

#nullable enable

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
                    CheckAvailability(value);
                }
            }
        }

        public IAsyncRelayCommand BrowseForFolderCommand { get; }

        public AddExistingVaultPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.Title = "Add existing vault";
            DialogViewModel.IsConfirmButtonVisible = true;

            DialogViewModel.ConfirmCommand = new RelayCommand(Confirm);
            BrowseForFolderCommand = new AsyncRelayCommand(BrowseForFolder);
        }

        private void Confirm()
        {
            DialogViewModel.VaultViewModel = new(new(), Path.GetDirectoryName(PathSourceText!)!);
        }

        private async Task BrowseForFolder()
        {
            var path = await FileExplorerService.PickSingleFileAsync(new List<string>() { Path.GetExtension(SecureFolderFS.Core.Constants.VAULT_CONFIGURATION_FILENAME) });
            if (!string.IsNullOrEmpty(path))
            {
                PathSourceText = path;
            }
        }

        private void CheckAvailability(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                DialogViewModel.IsConfirmButtonEnabled = false;
            }

            path = Path.Combine(Path.GetDirectoryName(path)!, SecureFolderFS.Core.Constants.VAULT_CONFIGURATION_FILENAME);
            if (File.Exists(path))
            {
                using var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

                var rawVaultConfiguration = RawVaultConfiguration.Load(fileStream);
                DialogViewModel.IsConfirmButtonEnabled = VaultVersion.IsVersionSupported((VaultVersion)rawVaultConfiguration);
            }
            else
            {
                DialogViewModel.IsConfirmButtonEnabled = false;
            }
        }
    }
}
