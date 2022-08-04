using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public abstract partial class VaultWizardPathSelectionBaseViewModel<TStorage> : BaseVaultWizardPageViewModel
        where TStorage : class, IStorable
    {
        protected IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        protected TStorage? SelectedLocation { get; set; }

        [ObservableProperty]
        private string? _LocationPath;

        /// <inheritdoc cref="DialogViewModel.PrimaryButtonEnabled"/>
        public bool PrimaryButtonEnabled
        {
            get => DialogViewModel.PrimaryButtonEnabled;
            set => DialogViewModel.PrimaryButtonEnabled = value;
        }

        protected VaultWizardPathSelectionBaseViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.PrimaryButtonEnabled = false;
        }

        public abstract Task<bool> SetLocation(TStorage storage);

        [RelayCommand]
        protected abstract Task BrowseLocationAsync(CancellationToken cancellationToken);
    }
}
