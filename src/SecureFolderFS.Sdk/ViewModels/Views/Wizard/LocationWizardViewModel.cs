using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IFileExplorerService>, Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class LocationWizardViewModel : BaseWizardViewModel
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _SelectedLocation;
        [ObservableProperty] private SeverityType _Severity;

        public NewVaultCreationType CreationType { get; }

        public IFolder? SelectedFolder { get; private set; }

        public LocationWizardViewModel(IVaultCollectionModel vaultCollectionModel, NewVaultCreationType creationType)
        {
            ServiceProvider = DI.Default;
            _vaultCollectionModel = vaultCollectionModel;
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
            // Remove previous bookmark
            if (SelectedFolder is IBookmark bookmark)
                await bookmark.RemoveBookmarkAsync(cancellationToken);
            
            SelectedFolder = await FileExplorerService.PickFolderAsync(true, cancellationToken);
            CanContinue = await UpdateStatusAsync(cancellationToken);
        }

        public async Task<bool> UpdateStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // No folder selected
                if (SelectedFolder is null)
                {
                    Severity = SeverityType.Default;
                    Message = "SelectFolderToContinue".ToLocalized();
                    return false;
                }

                // Check for duplicates
                var isDuplicate = _vaultCollectionModel.Any(x => x.Folder.Id == SelectedFolder.Id);
                if (isDuplicate)
                {
                    Severity = SeverityType.Warning;
                    Message = "VaultAlreadyExists".ToLocalized();
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
