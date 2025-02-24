using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public abstract partial class BasePreviewerViewModel<TSource> : ObservableObject, IAsyncInitialize, IViewable
        where TSource : class
    {
        /// <summary>
        /// Gets the data source of the previewer.
        /// </summary>
        [ObservableProperty] private TSource? _Source;

        /// <inheritdoc cref="IViewable.Title"/>
        [ObservableProperty] private string? _Title;

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
