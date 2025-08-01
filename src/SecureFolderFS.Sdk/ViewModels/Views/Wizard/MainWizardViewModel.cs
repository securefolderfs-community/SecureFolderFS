using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Bindable(true)]
    public sealed partial class MainWizardViewModel : OverlayViewModel, IStagingView
    {
        [ObservableProperty] private NewVaultMode _Mode;

        public MainWizardViewModel()
        {
            CanCancel = true;
            CanContinue = true;
            Title = "AddNewVault".ToLocalized();
            PrimaryText = "Continue".ToLocalized();
            SecondaryText = "Cancel".ToLocalized();
        }

        /// <inheritdoc/>
        public Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }
    }
}
