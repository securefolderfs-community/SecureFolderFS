using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage
{
    [Bindable(true)]
    public abstract partial class StorageItemViewModel : SelectableItemViewModel, IWrapper<IStorable>, IDisposable
    {
        /// <summary>
        /// Gets or sets the thumbnail image represented by <see cref="IImage"/> of this storage item, if any.
        /// </summary>
        [ObservableProperty] private IImage? _Thumbnail;

        /// <inheritdoc/>
        public abstract IStorable Inner { get; }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            Thumbnail?.Dispose();
        }
    }
}
