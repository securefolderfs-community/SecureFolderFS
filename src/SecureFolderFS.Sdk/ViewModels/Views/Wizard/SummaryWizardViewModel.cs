﻿using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Bindable(true)]
    public sealed partial class SummaryWizardViewModel : BaseWizardViewModel
    {
        private readonly IFolder _folder;
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _VaultName;

        public SummaryWizardViewModel(IFolder folder, IVaultCollectionModel vaultCollectionModel)
        {
            Title = "Summary".ToLocalized();
            CancelText = "Close".ToLocalized();
            ContinueText = null;
            CanContinue = true;
            CanCancel = true;
            VaultName = folder.Name;
            _folder = folder;
            _vaultCollectionModel = vaultCollectionModel;
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

        /// <inheritdoc/>
        public override async void OnAppearing()
        {
            // Add the newly created vault
            var vaultModel = new VaultModel(_folder);
            _vaultCollectionModel.Add(vaultModel);

            // Try to save the new vault
            var result = await _vaultCollectionModel.TrySaveAsync();
            _ = result; // TODO: Maybe use the result to indicate whether the save was successful or not

            // Display result as message
            Message = "VaultAdded".ToLocalized();
        }
    }
}
