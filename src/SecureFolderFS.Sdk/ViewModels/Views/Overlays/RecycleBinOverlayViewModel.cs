using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IRecycleBinService>]
    public sealed partial class RecycleBinOverlayViewModel : BaseDesignationViewModel, IAsyncInitialize
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;

        [ObservableProperty] private bool _IsRecycleBinEnabled;
        [ObservableProperty] private ObservableCollection<RecycleBinItemViewModel> _Items;

        public RecycleBinOverlayViewModel(UnlockedVaultViewModel unlockedVaultViewModel)
        {
            ServiceProvider = DI.Default;
            Items = new();
            Title = "RecycleBin".ToLocalized();
            _unlockedVaultViewModel = unlockedVaultViewModel;
        }
        
        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in RecycleBinService.GetRecycleBinItemsAsync(_unlockedVaultViewModel.StorageRoot, cancellationToken))
            {
                Items.Add(new(item.CiphertextItem, _unlockedVaultViewModel.StorageRoot, Items)
                {
                    Title = item.PlaintextName,
                    DeletionTimestamp = item.DeletionTimestamp
                });
            }

            IsRecycleBinEnabled = _unlockedVaultViewModel.StorageRoot.Options.IsRecycleBinEnabled;
        }

        [RelayCommand]
        private async Task ToggleRecycleBinAsync(CancellationToken cancellationToken)
        {
            var isEnabled = IsRecycleBinEnabled;
            var isSuccess = await RecycleBinService.ToggleRecycleBinAsync(
                _unlockedVaultViewModel.VaultViewModel.VaultModel.Folder,
                _unlockedVaultViewModel.StorageRoot,
                !isEnabled,
                cancellationToken);

            IsRecycleBinEnabled = !isSuccess ? isEnabled : !isEnabled;
        }

        [RelayCommand]
        private async Task RestoreItemsAsync(IEnumerable<RecycleBinItemViewModel>? items, CancellationToken cancellationToken)
        {
            if (items is null)
                return;

            foreach (var item in items.ToArray())
            {
                await RecycleBinService.RestoreItemAsync(_unlockedVaultViewModel.StorageRoot, item.CiphertextItem, cancellationToken);
                Items.Remove(item);
            }
        }
    }
}