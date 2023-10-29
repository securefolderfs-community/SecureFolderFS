using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed class ExplanationDialogViewModel : DialogViewModel, IAsyncInitialize
    {
        private readonly PeriodicTimer _periodicTimer;
        private int _elapsedTicks;

        public ExplanationDialogViewModel()
        {
            _periodicTimer = new(TimeSpan.FromSeconds(1));
            PrimaryButtonText = Constants.Dialogs.EXPLANATION_DIALOG_TIME_TICKS.ToString();
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            // We don't want to await it, since it's an async based timer
            _ = InitializeBlockingTimer(cancellationToken);

            return Task.CompletedTask;
        }

        private async Task InitializeBlockingTimer(CancellationToken cancellationToken)
        {
            while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                _elapsedTicks++;
                PrimaryButtonText = $"{Constants.Dialogs.EXPLANATION_DIALOG_TIME_TICKS - _elapsedTicks}";
                
                if (_elapsedTicks >= Constants.Dialogs.EXPLANATION_DIALOG_TIME_TICKS)
                {
                    PrimaryButtonText = "Close".ToLocalized();
                    PrimaryButtonEnabled = true;
                    _periodicTimer.Dispose();

                    break;
                }
            }
        }
    }
}
