using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IFileExplorerService>, Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class LocationWizardViewModel : BaseWizardViewModel
    {
        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _SelectedLocation;
        [ObservableProperty] private ViewSeverityType _Severity;

        public NewVaultCreationType CreationType { get; }

        public IFolder? SelectedFolder { get; private set; }

        public LocationWizardViewModel(NewVaultCreationType creationType)
        {
            ServiceProvider = DI.Default;
            CreationType = creationType;

            CanCancel = true;
            CanContinue = false;
            CancelText = "Cancel".ToLocalized();
            ContinueText = "Continue".ToLocalized();
            Title = creationType == NewVaultCreationType.AddExisting ? "AddExisting".ToLocalized() : "CreateNew".ToLocalized();
        }

        /// <inheritdoc/>
        public override Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            if (SelectedFolder is null)
                return Task.FromResult<IResult>(Result.Failure(null));

            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public override async void OnAppearing()
        {
            CanContinue = await UpdateStatusAsync();
        }

        [RelayCommand]
        private async Task SelectLocationAsync(CancellationToken cancellationToken)
        {
            SelectedFolder = await FileExplorerService.PickFolderAsync(cancellationToken);
            CanContinue = await UpdateStatusAsync(cancellationToken);
        }

        public async Task<bool> UpdateStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (SelectedFolder is null)
                {
                    Severity = ViewSeverityType.Default;
                    Message = "SelectFolderToContinue".ToLocalized();
                    return false;
                }

                // Validate vault
                var result = CreationType == NewVaultCreationType.AddExisting
                    ? await ValidationHelpers.ValidateExistingVault(SelectedFolder, cancellationToken)
                    : await ValidationHelpers.ValidateNewVault(SelectedFolder, cancellationToken);

                Severity = result.Value;
                Message = result.GetMessage();

                return result.Successful;
            }
            finally
            {
                SelectedLocation = SelectedFolder is null ? "SelectedNone".ToLocalized() : SelectedFolder.Name;
            }
        }
    }
}
