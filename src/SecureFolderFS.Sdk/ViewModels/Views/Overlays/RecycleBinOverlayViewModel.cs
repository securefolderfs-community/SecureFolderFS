using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IRecycleBinService>]
    public sealed partial class RecycleBinOverlayViewModel : BaseDesignationViewModel, IAsyncInitialize
    {
        [ObservableProperty] private bool _IsSelecting;
        [ObservableProperty] private bool _IsRecycleBinEnabled;
        [ObservableProperty] private ObservableCollection<RecycleBinItemViewModel> _Items;
        [ObservableProperty] private UnlockedVaultViewModel _UnlockedVaultViewModel;
        
        public INavigator OuterNavigator { get; }

        public RecycleBinOverlayViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigator outerNavigator)
        {
            ServiceProvider = DI.Default;
            Items = new();
            Title = "RecycleBin".ToLocalized();
            UnlockedVaultViewModel = unlockedVaultViewModel;
            OuterNavigator = outerNavigator;
        }
        
        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in RecycleBinService.GetRecycleBinItemsAsync(UnlockedVaultViewModel.StorageRoot, cancellationToken))
            {
                Items.Add(new(item.CiphertextItem, this)
                {
                    Title = item.PlaintextName,
                    DeletionTimestamp = item.DeletionTimestamp
                });
            }

            IsRecycleBinEnabled = UnlockedVaultViewModel.StorageRoot.Options.IsRecycleBinEnabled;
        }

        [RelayCommand]
        private async Task ToggleRecycleBinAsync(CancellationToken cancellationToken)
        {
            var isEnabled = IsRecycleBinEnabled;
            var isSuccess = await RecycleBinService.ToggleRecycleBinAsync(
                UnlockedVaultViewModel.VaultViewModel.VaultModel.Folder,
                UnlockedVaultViewModel.StorageRoot,
                !isEnabled,
                cancellationToken);

            IsRecycleBinEnabled = !isSuccess ? isEnabled : !isEnabled;
        }
        
        [RelayCommand]
        public void ToggleSelection(bool? value = null)
        {
            IsSelecting = value ?? !IsSelecting;
            Items.UnselectAll();
        }

        [RelayCommand]
        public void SelectAll()
        {
            IsSelecting = true;
            Items.SelectAll();
        }
    }
}