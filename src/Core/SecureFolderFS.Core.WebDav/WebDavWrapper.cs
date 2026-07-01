using Microsoft.Extensions.Logging;
using NWebDav.Server.Dispatching;
using SecureFolderFS.Core.WebDav.Helpers;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    public sealed class WebDavWrapper
    {
        private const int ERROR_OPERATION_ABORTED = 995;

        private Thread? _fsThead;
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
            var ts = new ThreadStart(async () => await EnsureFileSystemAsync());
            _fsThead = new Thread(ts);
            _fsThead.Start();
        }

        private async Task EnsureFileSystemAsync()
        {
            try
            {
                _httpListener.Start();
            }
            catch (Exception ex)
            {
                _requestDispatcher.Logger?.LogError(ex, "Failed to start the WebDAV HTTP listener.");
                return;
            }

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

            if (_mountPath is not null)
                DriveMappingHelpers.DisconnectNetworkDrive(_mountPath, true);

            return true;
        }
    }
}
