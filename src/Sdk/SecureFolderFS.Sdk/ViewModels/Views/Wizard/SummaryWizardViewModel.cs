using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Bindable(true)]
    public sealed partial class SummaryWizardViewModel : OverlayViewModel, IStagingView
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;
        private readonly IVaultModel _vaultModel;

        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _VaultName;
        [ObservableProperty] private bool _IsError;

        public SummaryWizardViewModel(IVaultModel vaultModel, IVaultCollectionModel vaultCollectionModel)
        {
            Title = "Summary".ToLocalized();
            SecondaryText = "Close".ToLocalized();
            PrimaryText = null;
            CanContinue = true;
            CanCancel = true;
            VaultName = vaultModel.DataModel.DisplayName;
            _vaultCollectionModel = vaultCollectionModel;
            _vaultModel = vaultModel;
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

        /// <inheritdoc/>
        public override async void OnAppearing()
        {
            // Add the newly created vault
            _vaultCollectionModel.Add(_vaultModel);

            // Try to save the new vault and reflect the outcome to the user. A silent failure here would
            // leave the vault absent after the next app restart, so it must be surfaced
            var saved = await _vaultCollectionModel.TrySaveAsync();
            IsError = !saved;
            Message = saved
                ? "VaultAdded".ToLocalized()
                : "VaultAddedSaveFailed".ToLocalized();
        }
    }
}
