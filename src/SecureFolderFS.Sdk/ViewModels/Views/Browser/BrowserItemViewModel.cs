using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    [Bindable(true)]
    public abstract partial class BrowserItemViewModel : ObservableObject, IWrapper<IStorable>, IViewable, IAsyncInitialize
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private IImage? _Thumbnail;

        /// <inheritdoc/>
        public abstract IStorable Inner { get; }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
        
        [RelayCommand]
        protected abstract Task OpenAsync(CancellationToken cancellationToken);
    }
}
