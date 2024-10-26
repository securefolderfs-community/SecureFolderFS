using NWebDav.Server.Dispatching;
using NWebDav.Server.HttpListener;
using SecureFolderFS.Core.WebDav.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    public sealed class WebDavWrapper
    {
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
                while (!_fileSystemCts.IsCancellationRequested && (await _httpListener.GetContextAsync() is var httpListenerContext))
                {
                    if (httpListenerContext.Request.IsAuthenticated)
                        Debugger.Break();

                    var context = new HttpContext(httpListenerContext);
                    await _requestDispatcher.DispatchRequestAsync(context, _fileSystemCts.Token);
                }
            }
            catch (Exception ex)
            {
                _ = ex;
                Debugger.Break();
            }
        }

        public async Task<bool> CloseFileSystemAsync()
        {
            _httpListener.Close();
            await _fileSystemCts.CancelAsync();

            if (_mountPath is not null)
                await DriveMappingHelpers.DisconnectNetworkDriveAsync(_mountPath, true);

            return true;
        }
    }
}
