using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.ViewModels.Views;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public abstract partial class BasePreviewerViewModel : BaseDesignationViewModel, IAsyncInitialize
    {
        /// <summary>
        /// Determines whether the toolbar should always be present on top.
        /// </summary>
        [ObservableProperty] private bool _IsToolbarOnTop;

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
