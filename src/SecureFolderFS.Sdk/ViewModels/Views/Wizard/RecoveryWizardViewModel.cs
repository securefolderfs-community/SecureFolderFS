using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IPrinterService>, Inject<IThreadingService>]
    public sealed partial class RecoveryWizardViewModel : BaseWizardViewModel
    {
        private readonly string? _vaultId;
        private readonly IDisposable? _unlockContract;

        public IFolder Folder { get; }

        public RecoveryPreviewControlViewModel RecoveryViewModel { get; }

        public RecoveryWizardViewModel(IFolder folder, IResult? additionalData)
        {
            ServiceProvider = Ioc.Default;
            Title = "VaultRecovery".ToLocalized();
            CanContinue = false;
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
                MasterKey = _unlockContract?.ToString()
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

        [RelayCommand]
        private void RevealMasterKey()
        {
            CanContinue = true;
        }
    }
}
