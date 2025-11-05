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
using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IVaultManagerService>, Inject<IClipboardService>]
    [Bindable(true)]
    public sealed partial class RecoveryOverlayViewModel : MessageOverlayViewModel, IDisposable
    {
        private readonly IFolder _vaultFolder;

        [ObservableProperty] private string? _RecoveryKey;
        [ObservableProperty] private string? _ErrorMessage;
        [ObservableProperty] private string? _OptionalNewPassword;

        public IDisposable? UnlockContract { get; private set; }

        public RecoveryOverlayViewModel(IFolder vaultFolder)
        {
            ServiceProvider = DI.Default;
            Title = "RecoverAccess".ToLocalized();
            Message = "EnterRecoveryKey".ToLocalized();
            _vaultFolder = vaultFolder;
        }

        public async Task<bool> RecoverAsync(CancellationToken cancellationToken)
        {
            try
            {
                var unlockContract = await VaultManagerService.RecoverAsync(_vaultFolder, RecoveryKey ?? string.Empty, cancellationToken);
                UnlockContract?.Dispose();
                UnlockContract = unlockContract;

                return true;
            }
            catch (Exception)
            {
                // TODO: Localize
                ErrorMessage = "The provided recovery key is invalid.";
                return false;
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
