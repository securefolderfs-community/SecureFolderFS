using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard2
{
    public abstract partial class BaseWizardViewModel : BasePageViewModel
    {
        [ObservableProperty] private bool _CanCancel;
        [ObservableProperty] private bool _CanContinue;

        public abstract Task<IResult> TryContinueAsync(CancellationToken cancellationToken);

        public abstract Task<IResult> TryCancelAsync(CancellationToken cancellationToken);
    }
}
