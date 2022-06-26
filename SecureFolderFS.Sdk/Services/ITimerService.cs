using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that measures and notifies when a certain time interval has passed.
    /// </summary>
    public interface ITimerService : IDisposable
    {
        /// <summary>
        /// Gets the timer interval in milliseconds.
        /// </summary>
        int Interval { get; }

        /// <summary>
        /// An async function delegate that is invoked on every tick.
        /// </summary>
        Func<CancellationToken, Task>? OnTickAsync { get; set; }

        /// <summary>
        /// A function delegate that is invoked on every tick.
        /// </summary>
        Action<CancellationToken>? OnTick { get; set; }

        /// <summary>
        /// Tries to set the interval.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        /// <returns>The value is true if the interval was set, otherwise false.</returns>
        bool SetInterval(int interval);

        /// <summary>
        /// Starts the timer and waits for ticks.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token that stops the timer.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task StartTimerAsync(CancellationToken cancellationToken);
    }
}
