using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard2
{
    public sealed partial class SummaryWizardViewModel : BaseWizardViewModel
    {
        [ObservableProperty] private string _VaultName;

        public SummaryWizardViewModel(string vaultName)
        {
            VaultName = vaultName;
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
