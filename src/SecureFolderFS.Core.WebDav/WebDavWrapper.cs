using NWebDav.Server.Dispatching;
using NWebDav.Server.HttpListener;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.WebDav.Helpers;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    internal sealed class WebDavWrapper
    {
        private readonly HttpListener _httpListener;
        private readonly IPrincipal? _serverPrincipal;
        private readonly IRequestDispatcher _requestDispatcher;
        private readonly CancellationTokenSource _fileSystemCts;
        private readonly string? _mountPath;

        public WebDavWrapper(HttpListener httpListener, IPrincipal? serverPrincipal, IRequestDispatcher requestDispatcher, string? mountPath = null)
        {
            _httpListener = httpListener;
            _serverPrincipal = serverPrincipal;
            _requestDispatcher = requestDispatcher;
            _fileSystemCts = new();
            _mountPath = mountPath;
        }

        public void StartFileSystem()
        {
            var ts = new ThreadStart(async () => await EnsureFileSystemAsync());
            var bgThread = new Thread(ts);
            bgThread.Start();
        }

        private async Task EnsureFileSystemAsync()
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

        public bool CloseFileSystem(FileSystemCloseMethod closeMethod)
        {
            _ = closeMethod; // TODO: Implement close method
            _fileSystemCts.Cancel();
            _httpListener.Close();

            if (_mountPath is not null)
                DriveMappingHelper.DisconnectNetworkDrive(_mountPath, true);

            return true;
        }
    }
}
