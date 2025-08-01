using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class PickerSourceWizardViewModel : BaseDataSourceWizardViewModel
    {
        private readonly IFolderPicker _folderPicker;
        private IFolder? _selectedFolder;

        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _SelectedLocation;
        [ObservableProperty] private Severity _Severity;

        /// <inheritdoc/>
        public override string DataSourceName { get; } = "SourceLocalStorage".ToLocalized();

        public PickerSourceWizardViewModel(IFolderPicker folderPicker, NewVaultMode creationType, IVaultCollectionModel vaultCollectionModel)
            : base(creationType, vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            _folderPicker = folderPicker;
            CanCancel = true;
            CanContinue = false;
            PrimaryText = "Continue".ToLocalized();
            SecondaryText = "Cancel".ToLocalized();
            Title = creationType == NewVaultMode.AddExisting ? "AddExisting".ToLocalized() : "CreateNew".ToLocalized();
        }

        /// <inheritdoc/>
        public override Task<IFolder?> GetFolderAsync()
        {
            return Task.FromResult(_selectedFolder);
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
            _selectedFolder = await _folderPicker.PickFolderAsync(null, true, cancellationToken);
            CanContinue = await UpdateStatusAsync(cancellationToken);
        }

        public async Task<bool> UpdateStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // No folder selected
                if (_selectedFolder is null)
                {
                    Severity = Severity.Default;
                    Message = "SelectFolderToContinue".ToLocalized();
                    return false;
                }

                // Check for duplicates
                var isDuplicate = VaultCollectionModel.Any(x => x.Folder.Id == _selectedFolder.Id);
                if (isDuplicate)
                {
                    Severity = Severity.Warning;
                    Message = "VaultAlreadyExists".ToLocalized();
                    return false;
                }

                // Validate vault
                var result = Mode == NewVaultMode.AddExisting
                    ? await ValidationHelpers.ValidateExistingVault(_selectedFolder, cancellationToken)
                    : await ValidationHelpers.ValidateNewVault(_selectedFolder, cancellationToken);

                Severity = result.Value;
                Message = result.GetMessage();

                return result.Successful;
            }
            finally
            {
                SelectedLocation = _selectedFolder is null ? "SelectedNone".ToLocalized() : _selectedFolder.Name;
            }
        }
    }
}
