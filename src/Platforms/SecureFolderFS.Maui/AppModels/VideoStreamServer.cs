using System.Net;
using System.Net.Sockets;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Maui.AppModels
{
    internal sealed class VideoStreamServer : IAsyncInitialize, IDisposable
    {
        private readonly HttpListener _httpListener;
        private readonly Stream _videoStream;
        private readonly string _mimeType;
        private readonly int _port;
        private bool _disposed;

        public string BaseAddress => $"http://localhost:{_port}/video";

        public VideoStreamServer(Stream videoStream, string mimeType)
        {
            _videoStream = videoStream;
            _mimeType = mimeType;

            // Automatically find a free port
            _port = GetAvailablePort();

            // Initialize the HttpListener with the free port
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://localhost:{_port}/");
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            _httpListener.Start();
            _ = BeginListeningAsync();

            return Task.CompletedTask;

            async Task BeginListeningAsync()
            {
                while (!_disposed && _httpListener.IsListening && await _httpListener.GetContextAsync() is var context)
                {
                    try
                    {
                        var response = context.Response;
                        if (context.Request.RawUrl != "/video")
                        {
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            response.Close();   
                            continue;
                        }
                        
                        response.ContentType = _mimeType;
                        response.ContentLength64 = _videoStream.Length;
                        response.Headers["Accept-Ranges"] = "bytes";

                        await _videoStream.CopyToAsync(response.OutputStream, 64 * 1024, cancellationToken);
                        await _videoStream.FlushAsync(cancellationToken);
                        _videoStream.Position = 0L;
                        
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.StatusDescription = "OK";
                        response.Close();
                    }
                    catch (Exception ex)
                    {
                        _ = ex;
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
            _videoStream.Dispose();
            _httpListener.Abort();
        }

        private static int GetAvailablePort()
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }
    }
}
