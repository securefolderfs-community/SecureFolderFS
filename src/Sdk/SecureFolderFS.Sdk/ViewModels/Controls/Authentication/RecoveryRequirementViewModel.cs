using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    /// <summary>
    /// Represents a view of a restored vault that has no credentials configured, where the recovery
    /// key is the only way in and new credentials must be set up before the vault can be used again.
    /// </summary>
    [Inject<IClipboardService>]
    [Bindable(true)]
    public sealed partial class RecoveryRequirementViewModel : ReportableViewModel
    {
        [ObservableProperty] private string? _RecoveryKey;
        [ObservableProperty] private string? _ErrorMessage;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public RecoveryRequirementViewModel()
        {
            ServiceProvider = DI.Default;
            Title = "SetCredentials".ToLocalized();
        }

        /// <inheritdoc/>
        public override void Report(IResult? result)
        {
            ErrorMessage = result is { Successful: false }
                ? result.GetMessage("UnknownError".ToLocalized())
                : null;
        }

        [RelayCommand]
        private void SetUpCredentials()
        {
            if (string.IsNullOrWhiteSpace(RecoveryKey))
                return;

            // The host performs the recovery and reports back through Report(), upon which
            // the vault is unlocked and new credentials can be registered
            ErrorMessage = null;
            StateChanged?.Invoke(this, new RecoveryRequestedEventArgs(RecoveryKey));
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
    }
}
