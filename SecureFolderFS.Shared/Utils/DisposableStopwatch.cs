using System;
using System.Diagnostics;

namespace SecureFolderFS.Shared.Utils
{
    public sealed class DisposableStopwatch : IDisposable
    {
        private readonly Stopwatch _stopwatch;

        private readonly Action<Stopwatch>? _onFinishedCallback;

        public DisposableStopwatch(Action<Stopwatch>? onFinishedCallback)
        {
            this._onFinishedCallback = onFinishedCallback;
            this._stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _onFinishedCallback?.Invoke(_stopwatch);
        }
    }
}
