using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NWebDav.Server.Dispatching;
using SecureFolderFS.Core.WebDav.Helpers;

namespace SecureFolderFS.Core.WebDav
{
    public sealed class WebDavWrapper
    {
        private const int ERROR_OPERATION_ABORTED = 995;

        private Task? _acceptLoopTask;
        private readonly HttpListener _httpListener;
        private readonly IRequestDispatcher _requestDispatcher;
        private readonly CancellationTokenSource _fileSystemCts;
        private readonly string? _mountPath;

        public WebDavWrapper(HttpListener httpListener, IRequestDispatcher requestDispatcher, string? mountPath = null)
        {
            _httpListener = httpListener;
            _requestDispatcher = requestDispatcher;
            _fileSystemCts = new();
            _mountPath = mountPath;
        }

        public void StartFileSystem()
        {
            // The accept loop is fully asynchronous and I/O-bound (each request is dispatched onto the thread pool).
            // Running it as a background task keeps the UI responsive without occupying a thread
            _acceptLoopTask = Task.Run(EnsureFileSystemAsync);
        }

        private async Task EnsureFileSystemAsync()
        {
            // The caller starts the listener before mounting, so a bind failure has already surfaced
            while (!_fileSystemCts.IsCancellationRequested)
            {
                HttpListenerContext httpListenerContext;
                try
                {
                    httpListenerContext = await _httpListener.GetContextAsync();
                }
                catch (Exception ex) when (ex is ObjectDisposedException or HttpListenerException { ErrorCode: ERROR_OPERATION_ABORTED })
                {
                    // The listener was closed, stop accepting requests
                    break;
                }
                catch (Exception ex)
                {
                    // A transient error while accepting a request must not kill the accept loop
                    _requestDispatcher.Logger?.LogError(ex, "Failed to accept an incoming WebDAV request.");
                    continue;
                }

                // Dispatch each request concurrently. The Windows WebDAV redirector interleaves requests
                // (PROPFIND, LOCK refreshes) with long-running transfers and times out when they stall.
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _requestDispatcher.DispatchRequestAsync(httpListenerContext, _fileSystemCts.Token);
                    }
                    catch (Exception ex)
                    {
                        _requestDispatcher.Logger?.LogError(ex, "Unhandled exception while dispatching a WebDAV request.");
                    }
                }, _fileSystemCts.Token);
            }
        }

        public async Task<bool> CloseFileSystemAsync()
        {
            _httpListener.Close();
            await _fileSystemCts.CancelAsync();

            // Wait for the accept loop to observe the closed listener and unwind
            if (_acceptLoopTask is not null)
            {
                try
                {
                    await _acceptLoopTask;
                }
                catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
                {
                    // Expected during teardown
                }
            }

            if (_mountPath is not null)
                DriveMappingHelpers.DisconnectNetworkDrive(_mountPath, true);

            return true;
        }
    }
}
