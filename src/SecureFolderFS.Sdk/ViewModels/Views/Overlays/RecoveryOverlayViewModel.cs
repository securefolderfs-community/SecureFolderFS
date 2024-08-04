using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IVaultManagerService>, Inject<IClipboardService>]
    [Bindable(true)]
    public sealed partial class RecoveryOverlayViewModel : OverlayViewModel, IDisposable
    {
        private readonly IFolder _vaultFolder;

        [ObservableProperty] private string? _MasterKey;
        [ObservableProperty] private string? _ErrorMessage;

        public IDisposable? UnlockContract { get; private set; }

        public RecoveryOverlayViewModel(IFolder vaultFolder)
        {
            ServiceProvider = DI.Default;
            _vaultFolder = vaultFolder;
        }

        public async Task<bool> RecoverAsync(CancellationToken cancellationToken)
        {
            try
            {
                var unlockContract = await VaultManagerService.RecoverAsync(_vaultFolder, MasterKey ?? string.Empty, cancellationToken);
                UnlockContract?.Dispose();
                UnlockContract = unlockContract;

                return true;
            }
            catch (Exception)
            {
                ErrorMessage = "The provided master key is invalid";
                return false;
            }
        }

        [RelayCommand]
        private async Task PasteMasterKeyAsync(CancellationToken cancellationToken)
        {
            try
            {
                MasterKey = await ClipboardService.GetTextAsync(cancellationToken) ?? MasterKey;
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
