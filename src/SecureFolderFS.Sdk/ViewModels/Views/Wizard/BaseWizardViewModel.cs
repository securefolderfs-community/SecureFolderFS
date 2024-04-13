using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    public abstract partial class BaseWizardViewModel : ObservableObject, IViewDesignation
    {
        /// <inheritdoc cref="IViewable.Title"/>
        [ObservableProperty] private string? _Title;
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
