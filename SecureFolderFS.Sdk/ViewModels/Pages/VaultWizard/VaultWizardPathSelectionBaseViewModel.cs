using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public abstract class VaultWizardPathSelectionBaseViewModel<TStorage> : BaseVaultWizardPageViewModel
        where TStorage : class, IBaseStorage
    {
        protected IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        protected TStorage? SelectedLocation { get; set; }

        private string? _LocationPath;
        public string? LocationPath
        {
            get => _LocationPath;
            set => SetProperty(ref _LocationPath, value);
        }

        /// <inheritdoc cref="DialogViewModel.PrimaryButtonEnabled"/>
        public bool PrimaryButtonEnabled
        {
            get => DialogViewModel.PrimaryButtonEnabled;
            set => DialogViewModel.PrimaryButtonEnabled = value;
        }

        public IAsyncRelayCommand BrowseLocationCommand { get; }

        protected VaultWizardPathSelectionBaseViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.PrimaryButtonEnabled = false;
            BrowseLocationCommand = new AsyncRelayCommand(BrowseLocationAsync);
        }

        public abstract Task<bool> SetLocation(TStorage storage);

        protected abstract Task BrowseLocationAsync();
    }
}
