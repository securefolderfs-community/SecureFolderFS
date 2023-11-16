using System;
using System.Diagnostics;

namespace SecureFolderFS.Shared.Helpers
{
    public sealed class DisposableStopwatch : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly Action<Stopwatch>? _onFinished;

        public DisposableStopwatch(Action<Stopwatch>? onFinished)
        {
            _onFinished = onFinished;
            _stopwatch = new();
            _stopwatch.Start();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _stopwatch.Stop();
            if (_onFinished is not null)
            {
                _onFinished.Invoke(_stopwatch);
            }
            else
            {
                DebuggingHelpers.OutputToDebug($"{nameof(DisposableStopwatch)} took: {_stopwatch.ElapsedMilliseconds}ms");
            }
        }
    }
}
