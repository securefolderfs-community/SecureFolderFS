using SecureFolderFS.Core.FileSystem.Enums;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.WebDav.Http;
using SecureFolderFS.Core.WebDav.Http.Context;

namespace SecureFolderFS.Core.WebDav
{
    internal sealed class WebDavWrapper
    {
        private Task? _fileSystemTask;
        private readonly HttpListener _httpListener;
        private readonly IDavDispatcher _davDispatcher;
        private readonly CancellationTokenSource _fileSystemCts;
        private readonly IHttpSession _httpSession;

        public WebDavWrapper(HttpListener httpListener, IDavDispatcher davDispatcher, IPrincipal? serverPrincipal)
        {
            _httpListener = httpListener;
            _davDispatcher = davDispatcher;
            _httpSession = new HttpSession(serverPrincipal);
            _fileSystemCts = new();
        }

        public void StartFileSystem()
        {
            _httpListener.Start();
            _fileSystemTask = Task.Run<Task>(async () =>
            {
                while (!_fileSystemCts.IsCancellationRequested && (await _httpListener.GetContextAsync() is var httpListenerContext))
                {
                    if (httpListenerContext.Request.IsAuthenticated)
                        Debugger.Break();

                    var context = new HttpContext(httpListenerContext.Request, httpListenerContext.Response, _httpSession);
                    await _davDispatcher.DispatchAsync(context, _fileSystemCts.Token);
                }
            }).Unwrap();
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
