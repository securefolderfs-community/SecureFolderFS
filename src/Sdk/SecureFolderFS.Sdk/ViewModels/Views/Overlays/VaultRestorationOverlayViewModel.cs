using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IVaultManagerService>, Inject<IClipboardService>]
    public sealed partial class VaultRestorationOverlayViewModel : OverlayViewModel, IDisposable
    {
        private readonly IFolder _vaultFolder;

        [ObservableProperty] private string? _RecoveryKey;

        public IDisposable? UnlockContract { get; private set; }

        public VaultRestorationOverlayViewModel(IFolder vaultFolder)
        {
            ServiceProvider = DI.Default;
            _vaultFolder = vaultFolder;
        }

        public async Task<IResult> RestoreAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(RecoveryKey))
                return Result.Failure(null);

            try
            {
                UnlockContract = await VaultManagerService.RestoreAsync(_vaultFolder, RecoveryKey, cancellationToken);
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }

        [RelayCommand]
        private async Task PasteRecoveryKeyAsync(CancellationToken cancellationToken)
        {
            try
            {
                RecoveryKey = await ClipboardService.GetTextAsync(cancellationToken) ?? RecoveryKey;
            }
            catch (FormatException) { }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            UnlockContract?.Dispose();
        }
    }
}