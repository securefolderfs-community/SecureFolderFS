using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    public abstract partial class StorageItemViewModel : ObservableObject, IViewable, IAsyncInitialize
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private IImage? _Thumbnail;

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
