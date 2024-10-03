using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IPrinterService>, Inject<IThreadingService>]
    [Bindable(true)]
    public sealed partial class RecoveryWizardViewModel : BaseWizardViewModel
    {
        private readonly string? _vaultId;
        private readonly IDisposable? _unlockContract;

        [ObservableProperty] private RecoveryPreviewControlViewModel _RecoveryViewModel;

        public IFolder Folder { get; }

        public RecoveryWizardViewModel(IFolder folder, IResult? additionalData)
        {
            ServiceProvider = DI.Default;
            Title = "VaultRecovery".ToLocalized();
            ContinueText = "Continue".ToLocalized();
            CancelText = "Cancel".ToLocalized();
            CanContinue = true;
            CanCancel = false;
            Folder = folder;

            if (additionalData is CredentialsResult result)
            {
                _unlockContract = result.Value;
                _vaultId = result.VaultId;
            }

            RecoveryViewModel = new()
            {
                VaultId = _vaultId,
                VaultName = Folder.Name,
                RecoveryKey = _unlockContract?.ToString()
            };
        }

        /// <inheritdoc/>
        public override Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Failure(null));
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
        {
            _unlockContract?.Dispose();
        }
    }
}
