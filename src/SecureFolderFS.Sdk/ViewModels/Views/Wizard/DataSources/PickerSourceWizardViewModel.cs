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
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Pickers;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

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

        public PickerSourceWizardViewModel(string sourceId, IFolderPicker folderPicker, NewVaultMode mode, IVaultCollectionModel vaultCollectionModel)
            : base(sourceId, mode, vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            _folderPicker = folderPicker;
            CanCancel = true;
            CanContinue = false;
            PrimaryText = "Continue".ToLocalized();
            SecondaryText = "Cancel".ToLocalized();
            Title = mode == NewVaultMode.AddExisting ? "AddExisting".ToLocalized() : "CreateNew".ToLocalized();
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
            var result = await ValidationHelpers.ValidateAddedVault(_selectedFolder, Mode, VaultCollectionModel, cancellationToken);

            Message = result.Message;
            Severity = result.Severity;
            SelectedLocation = result.SelectedLocation;

            return result.CanContinue;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            (_folderPicker as IDisposable)?.Dispose();
        }
    }
}
