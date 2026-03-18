using System.Threading.Channels;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Maui.Helpers
{
    internal sealed class DeferredInitialization<T> : IDisposable
        where T : notnull
    {
        private T? _context;
        private readonly int _maxParallelization;
        private readonly Channel<IAsyncInitialize> _channel;
        private Task? _processingTask;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public DeferredInitialization(int maxParallelization)
        {
            _maxParallelization = maxParallelization;
            _channel = Channel.CreateUnbounded<IAsyncInitialize>(
                new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = false });
        }

        public void SetContext(T context)
        {
            if (context.Equals(_context))
                return;

            _context = context;

            // Drain the channel without blocking
            while (_channel.Reader.TryRead(out _)) { }
        }

        public void Enqueue(IAsyncInitialize asyncInitialize)
        {
            if (_context is null)
                return;

            _channel.Writer.TryWrite(asyncInitialize);
            EnsureProcessing();
        }

        private void EnsureProcessing()
        {
            if (_processingTask is { IsCompleted: false })
                return;

            _semaphore.Wait();
            try
            {
                if (_processingTask is { IsCompleted: false })
                    return;

                _processingTask = Task.Run(ProcessLoopAsync);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ProcessLoopAsync()
        {
            var batch = new List<IAsyncInitialize>(_maxParallelization);

            while (await _channel.Reader.WaitToReadAsync())
            {
                batch.Clear();
                while (batch.Count < _maxParallelization && _channel.Reader.TryRead(out var item))
                    batch.Add(item);

                if (batch.Count == 0)
                    continue;

                await Task.WhenAll(batch.Select(init => init.InitAsync()));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _channel.Writer.TryComplete();
            _semaphore.Dispose();
        }
    }
}
