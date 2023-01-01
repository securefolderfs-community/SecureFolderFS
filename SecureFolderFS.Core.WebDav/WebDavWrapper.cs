using SecureFolderFS.Core.FileSystem.Enums;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    internal class WebDavWrapper
    {
        private Task? _fileSystemTask;
        private readonly HttpListener _httpListener;
        private readonly CancellationTokenSource _fileSystemCts;

        public WebDavWrapper(HttpListener httpListener)
        {
            _httpListener = httpListener;
            _fileSystemCts = new();
        }

        public void StartFileSystem()
        {
            _fileSystemTask = Task.Run<Task>(async () =>
            {
                while (!_fileSystemCts.IsCancellationRequested && (await _httpListener.GetContextAsync() is var httpListenerContext))
                {
                    if (httpListenerContext.Request.IsAuthenticated)
                        Debugger.Break();

                    //httpListenerContext
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
