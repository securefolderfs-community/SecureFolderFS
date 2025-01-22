using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Bindable(true)]
    public abstract partial class BaseWizardViewModel : ObservableObject, IViewDesignation // TODO: Use IOverlayControls
    {
        /// <inheritdoc cref="IViewable.Title"/>
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private string? _CancelText;
        [ObservableProperty] private string? _ContinueText;
        [ObservableProperty] private bool _CanCancel;
        [ObservableProperty] private bool _CanContinue;

        /// <inheritdoc/>
        public virtual void OnAppearing()
        {
        }

        /// <inheritdoc/>
        public virtual void OnDisappearing()
        {
        }

        // TODO: Needs docs
        public abstract Task<IResult> TryContinueAsync(CancellationToken cancellationToken);

        public abstract Task<IResult> TryCancelAsync(CancellationToken cancellationToken);
    }
}
