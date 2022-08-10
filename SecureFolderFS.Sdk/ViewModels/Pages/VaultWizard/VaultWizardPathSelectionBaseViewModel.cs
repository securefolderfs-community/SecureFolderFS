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
        private string? _VaultName = "No folder selected";

        protected VaultWizardPathSelectionBaseViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            DialogViewModel.PrimaryButtonEnabled = false;
        }

        public abstract Task<bool> SetLocationAsync(TStorage storage, CancellationToken cancellationToken = default);

        [RelayCommand]
        protected abstract Task BrowseLocationAsync(CancellationToken cancellationToken);
    }
}
