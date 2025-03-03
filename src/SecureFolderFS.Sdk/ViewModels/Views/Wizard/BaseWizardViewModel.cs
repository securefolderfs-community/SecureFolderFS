using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Bindable(true)]
    public abstract class BaseWizardViewModel : OverlayViewModel // TODO: Use IOverlayControls
    {
        // TODO: Needs docs
        public abstract Task<IResult> TryContinueAsync(CancellationToken cancellationToken);

        public abstract Task<IResult> TryCancelAsync(CancellationToken cancellationToken);
    }
}
