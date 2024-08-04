using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Bindable(true)]
    public sealed partial class MainWizardViewModel : BaseWizardViewModel
    {
        [ObservableProperty] private NewVaultCreationType _CreationType;

        public MainWizardViewModel()
        {
            CanCancel = true;
            CanContinue = true;
            Title = "AddNewVault".ToLocalized();
            CancelText = "Cancel".ToLocalized();
            ContinueText = "Continue".ToLocalized();
        }

        /// <inheritdoc/>
        public override Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }
    }
}
