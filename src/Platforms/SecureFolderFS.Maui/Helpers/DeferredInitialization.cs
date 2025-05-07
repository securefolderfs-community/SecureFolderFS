using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Maui.Helpers
{
    internal sealed class DeferredInitialization<T> : IDisposable
        where T : notnull
    {
        private T? _context;
        private bool _isProcessing;
        private readonly List<IAsyncInitialize> _initializations = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public void SetContext(T context)
        {
            if (context.Equals(_context))
                return;

            _context = context;
            lock (_initializations)
                _initializations.Clear();
        }

        public void Enqueue(IAsyncInitialize asyncInitialize)
        {
            if (_context is null)
                return;

            lock (_initializations)
                _initializations.Add(asyncInitialize);

            _ = StartProcessingAsync();
        }

        private async Task StartProcessingAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                if (_isProcessing)
                    return;

                _isProcessing = true;
            }
            finally
            {
                _semaphore.Release();
            }

            try
            {
                while (true)
                {
                    IAsyncInitialize[] batch;
                    lock (_initializations)
                    {
                        if (_initializations.Count == 0)
                            break;

                        batch = _initializations.Take(UI.Constants.Browser.THUMBNAIL_MAX_PARALLELISATION).ToArray();
                        _initializations.RemoveRange(0, batch.Length);
                    }

                    var tasks = batch.Select(init => init.InitAsync());
                    await Task.Run(async () => await Task.WhenAll(tasks));
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _initializations.Clear();
            _semaphore.Dispose();
        }
    }
}
