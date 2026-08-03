using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
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
        [ObservableProperty] private bool _IsAwaitingConfirmation;
        [ObservableProperty] private VaultRestorationParameters? _DetectedParameters;
        [ObservableProperty] private string? _DetectedFileNameCipher;
        [ObservableProperty] private bool _IsFileNameEncryptionMissing;

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
                UnlockContract = await VaultManagerService.RestoreAsync(_vaultFolder, RecoveryKey, ConfirmParametersAsync, cancellationToken);
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }

        /// <summary>
        /// Presents the parameters detected for the vault and waits for the user to accept them.
        /// </summary>
        /// <remarks>
        /// The first pass surfaces the parameters and reports back that they are not confirmed, which
        /// leaves the dialog open showing them; accepting re-runs the restoration, and the second pass confirms.
        /// </remarks>
        private Task<bool> ConfirmParametersAsync(VaultRestorationParameters parameters, CancellationToken cancellationToken)
        {
            if (IsAwaitingConfirmation)
                return Task.FromResult(true);

            DetectedParameters = parameters;

            // A vault detected as having no filename encryption must be notified about
            IsFileNameEncryptionMissing = !parameters.IsFileNameEncrypted;
            DetectedFileNameCipher = parameters.IsFileNameEncrypted
                ? $"{parameters.FileNameCipherId} ({parameters.FileNameEncodingId})"
                : "NoEncryption".ToLocalized();

            IsAwaitingConfirmation = true;

            return Task.FromResult(false);
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