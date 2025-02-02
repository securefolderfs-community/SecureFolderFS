﻿using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed class ExplanationOverlayViewModel : OverlayViewModel, IAsyncInitialize
    {
        private readonly PeriodicTimer _periodicTimer;
        private int _elapsedTicks;

        public ExplanationOverlayViewModel()
        {
            _periodicTimer = new(TimeSpan.FromSeconds(1));
            PrimaryButtonText = Constants.Dialogs.EXPLANATION_DIALOG_TIME_TICKS.ToString();
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            // We don't want to await it, since the timer is supposed to run in the background
            _ = InitializeBlockingTimer(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task InitializeBlockingTimer(CancellationToken cancellationToken)
        {
            while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                _elapsedTicks++;
                PrimaryButtonText = $"{Constants.Dialogs.EXPLANATION_DIALOG_TIME_TICKS - _elapsedTicks}";

#if !DEBUG // Skip waiting if debugging
                if (_elapsedTicks >= Constants.Dialogs.EXPLANATION_DIALOG_TIME_TICKS)
#endif
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
