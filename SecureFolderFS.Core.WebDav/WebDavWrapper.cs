using NWebDav.Server.Dispatching;
using NWebDav.Server.HttpListener;
using SecureFolderFS.Core.FileSystem.Enums;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    internal sealed class WebDavWrapper
    {
        private Task? _fileSystemTask;
        private readonly HttpListener _httpListener;
        private readonly IPrincipal? _serverPrincipal;
        private readonly IRequestDispatcher _requestDispatcher;
        private readonly CancellationTokenSource _fileSystemCts;

        public WebDavWrapper(HttpListener httpListener, IPrincipal? serverPrincipal, IRequestDispatcher requestDispatcher)
        {
            _httpListener = httpListener;
            _serverPrincipal = serverPrincipal;
            _requestDispatcher = requestDispatcher;
            _fileSystemCts = new();
        }

        public void StartFileSystem()
        {
            _httpListener.Start();
            _fileSystemTask = EnsureFileSystemAsync();
        }

        private async Task EnsureFileSystemAsync()
        {
            while (!_fileSystemCts.IsCancellationRequested && (await _httpListener.GetContextAsync() is var httpListenerContext))
            {
                if (httpListenerContext.Request.IsAuthenticated)
                    Debugger.Break();

                var context = new HttpContext(httpListenerContext);
                await _requestDispatcher.DispatchRequestAsync(context); // TODO(wd): _fileSystemCts.Token
            }
        }

        public bool CloseFileSystem(FileSystemCloseMethod closeMethod)
        {
            _ = closeMethod; // TODO: Implement close method
            _fileSystemCts.Cancel();
            _fileSystemTask?.Dispose();

            return true;
        }
    }
}
