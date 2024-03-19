using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
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
        private readonly IDisposable? _superSecret;

        [ObservableProperty] private string? _MasterKey;

        public IFolder Folder { get; }

        public RecoveryWizardViewModel(IFolder folder, IResult? additionalData)
        {
            ServiceProvider = Ioc.Default;
            Title = "VaultRecovery".ToLocalized();
            CanContinue = true;
            CanCancel = false;
            Folder = folder;

            if (additionalData is CredentialsResult result)
            {
                _superSecret = result.Value;
                _vaultId = result.VaultId;
            }
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
            _superSecret?.Dispose();
            MasterKey = null;
        }

        [RelayCommand]
        private async Task PrintAsync(CancellationToken cancellationToken)
        {
            await ThreadingService.ChangeThreadAsync();

            if (await PrinterService.IsSupportedAsync())
                await PrinterService.PrintMasterKeyAsync(Folder.Name, _vaultId, _superSecret);
        }


        [RelayCommand]
        private void RevealMasterKey()
        {
            MasterKey ??= _superSecret?.ToString() ?? "No masterkey to show";
        }
    }
}
