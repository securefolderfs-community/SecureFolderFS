using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="ITimerService"/>
    internal sealed class TimerService : ITimerService
    {
        private PeriodicTimer? _periodicTimer;

        /// <inheritdoc/>
        public int Interval { get; private set; }

        /// <inheritdoc/>
        public Func<CancellationToken, Task>? OnTickAsync { get; set; }

        /// <inheritdoc/>
        public Action<CancellationToken>? OnTick { get; set; }

        /// <inheritdoc/>
        public bool SetInterval(int interval)
        {
            _periodicTimer?.Dispose();
            _periodicTimer = new(TimeSpan.FromMilliseconds(interval));
            Interval = interval;

            return true;
        }

        /// <inheritdoc/>
        public async Task StartTimerAsync(CancellationToken cancellationToken)
        {
            _ = _periodicTimer ?? throw new InvalidOperationException("The interval has not been set.");

            while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                OnTick?.Invoke(cancellationToken);
                if (OnTickAsync is not null)
                    await OnTickAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _periodicTimer?.Dispose();
        }
    }
}
