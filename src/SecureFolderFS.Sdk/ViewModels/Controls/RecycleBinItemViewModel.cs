using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Bindable(true)]
    [Inject<IRecycleBinService>]
    public sealed partial class RecycleBinItemViewModel : ObservableObject, IViewable
    {
        private readonly IVFSRoot _vfsRoot;
        private readonly ICollection<RecycleBinItemViewModel> _recycleBinItems;
        
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private DateTime? _DeletionTimestamp;
        
        /// <summary>
        /// Gets the storage object that represents a singular item within the Recycle Bin.
        /// </summary>
        public IStorableChild CiphertextItem { get; }

        public RecycleBinItemViewModel(IStorableChild ciphertextItem, IVFSRoot vfsRoot, ICollection<RecycleBinItemViewModel> recycleBinItems)
        {
            ServiceProvider = DI.Default;
            CiphertextItem = ciphertextItem;
            _vfsRoot = vfsRoot;
            _recycleBinItems = recycleBinItems;
        }

        [RelayCommand]
        private async Task RecoverAsync(CancellationToken cancellationToken)
        {
            var result = await RecycleBinService.RestoreItemAsync(_vfsRoot, CiphertextItem, cancellationToken);
            if (result.Successful)
                _recycleBinItems.Remove(this);
        }
    }
}
