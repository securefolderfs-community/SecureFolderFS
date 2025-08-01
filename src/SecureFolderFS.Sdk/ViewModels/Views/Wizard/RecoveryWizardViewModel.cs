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
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IPrinterService>, Inject<IThreadingService>]
    [Bindable(true)]
    public sealed partial class RecoveryWizardViewModel : OverlayViewModel, IStagingView
    {
        private readonly IDisposable? _unlockContract;

        [ObservableProperty] private RecoveryPreviewControlViewModel _RecoveryViewModel;

        public IFolder Folder { get; }

        public RecoveryWizardViewModel(IFolder folder, CredentialsResult result)
        {
            ServiceProvider = DI.Default;
            Title = "VaultRecovery".ToLocalized();
            PrimaryText = "Continue".ToLocalized();
            SecondaryText = "Cancel".ToLocalized();
            CanContinue = true;
            CanCancel = false;
            Folder = folder;
            _unlockContract = result.Value;

            RecoveryViewModel = new()
            {
                VaultId = result.VaultId,
                Title = Folder.Name,
                RecoveryKey = _unlockContract?.ToString()
            };
        }

        /// <inheritdoc/>
        public Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
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
