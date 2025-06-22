using System.Net;
using System.Net.Sockets;
using Android.Content.Res;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Maui.AppModels
{
    internal sealed class PdfStreamServer : IAsyncInitialize, IDisposable
    {
        private readonly HttpListener _httpListener;
        private readonly Stream _fileStream;
        private readonly string _mimeType;
        private readonly int _port;
        private bool _disposed;

        public string BaseAddress => $"http://localhost:{_port}";

        public PdfStreamServer(Stream fileStream, string mimeType)
        {
            if (!fileStream.CanSeek)
                throw new ArgumentException("Stream must be seekable.", nameof(fileStream));
            
            _fileStream = fileStream;
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
                try
                {
                    while (!_disposed && _httpListener.IsListening && await _httpListener.GetContextAsync() is var context)
                    {
                        var response = context.Response;
                        var absolutePath = context.Request.Url?.AbsolutePath ?? string.Empty;
                        
                        try
                        {
                            if (absolutePath == "/app_file")
                            {
                                response.ContentType = _mimeType;
                                response.Headers["Accept-Ranges"] = "bytes";
                                response.ContentLength64 = _fileStream.Length;

                                await _fileStream.CopyToAsync(response.OutputStream, cancellationToken);
                                if (_fileStream.CanSeek)
                                    _fileStream.Position = 0L;
                                
                                response.StatusCode = (int)HttpStatusCode.OK;
                                response.StatusDescription = "OK";
                            }
                            else if (absolutePath.StartsWith("/pdfjs53"))
                            {
                                var relativePath = absolutePath.TrimStart('/');
                                var contentType = FileTypeHelper.GetMimeType(relativePath);
                                response.ContentType = contentType;
                                response.Headers["Accept-Ranges"] = "bytes";
                                
                                await using var assetStream = Android.App.Application.Context.Assets?.Open(relativePath, Access.Random);
                                if (assetStream is null)
                                {
                                    response.StatusCode = (int)HttpStatusCode.NotFound;
                                    continue;
                                }
                                
                                // All this double-copying of data is needed for setting the ContentLength tag
                                
                                // Copy to temporary MemoryStream
                                await using var memoryStream = new MemoryStream();
                                await assetStream.CopyToAsync(memoryStream, cancellationToken);
                                memoryStream.Position = 0L;
                                
                                // Set the ContentLength tag
                                response.ContentLength64 = memoryStream.Length;
                                
                                // Copy back to the OutputStream
                                await memoryStream.CopyToAsync(response.OutputStream, cancellationToken);
                                response.StatusCode = (int)HttpStatusCode.OK;
                                response.StatusDescription = "OK";
                            }
                        }
                        finally
                        {
                            response.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = ex;
                }
            }
        }
        
        private static string GetMimeType(string ext) => ext.ToLowerInvariant() switch
        {
            ".html" => "text/html",
            ".js" => "application/javascript",
            ".mjs" => "text/javascript", // important for modern JS modules
            ".css" => "text/css",
            ".json" => "application/json",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            ".ttf" => "font/ttf",
            ".eot" => "application/vnd.ms-fontobject",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
            _fileStream.Dispose();
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
