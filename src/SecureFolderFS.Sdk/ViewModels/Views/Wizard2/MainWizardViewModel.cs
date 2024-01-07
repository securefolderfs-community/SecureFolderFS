using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard2
{
    public sealed partial class MainWizardViewModel : BaseWizardViewModel
    {
        [ObservableProperty] private NewVaultCreationType _CreationType;

        public MainWizardViewModel()
        {
            CanContinue = true;
            CanCancel = true;
        }

        /// <inheritdoc/>
        public override Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(CommonResult.Success);
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(CommonResult.Success);
        }
    }
}
