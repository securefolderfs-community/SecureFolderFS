using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IVaultFileSystemService>]
    public sealed partial class RecycleBinOverlayViewModel : BaseDesignationViewModel, IAsyncInitialize
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        
        [ObservableProperty] private ObservableCollection<RecycleBinItemViewModel> _Items;

        public RecycleBinOverlayViewModel(UnlockedVaultViewModel unlockedVaultViewModel)
        {
            ServiceProvider = DI.Default;
            Items = new();
            _unlockedVaultViewModel = unlockedVaultViewModel;
        }
        
        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in VaultFileSystemService.GetRecycleBinItemsAsync(
                               _unlockedVaultViewModel.StorageRoot, cancellationToken))
            {
                Items.Add(item);
            }
        }

        [RelayCommand]
        private async Task RestoreItemsAsync(IEnumerable<RecycleBinItemViewModel>? items, CancellationToken cancellationToken)
        {
            if (items is null)
                return;

            foreach (var item in items.ToArray())
            {
                if (item.CiphertextItem is not IStorableChild storableChild)
                    continue;
                
                await VaultFileSystemService.RestoreItemAsync(_unlockedVaultViewModel.StorageRoot, storableChild, cancellationToken);
                Items.Remove(item);
            }
        }
    }
}