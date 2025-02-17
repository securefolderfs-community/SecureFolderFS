using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Bindable(true)]
    public sealed partial class RecycleBinItemViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private DateTime? _DeletionTimestamp;
        
        /// <summary>
        /// Gets the storage object that represents a singular item within the Recycle Bin.
        /// </summary>
        public IStorableChild CiphertextItem { get; }

        public RecycleBinItemViewModel(IStorableChild ciphertextItem)
        {
            CiphertextItem = ciphertextItem;
        }
    }
}
