using System.Net;
using System.Net.Sockets;
using System.Text;
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
                                await memoryStream.FlushAsync(cancellationToken);
                                memoryStream.Position = 0L;

                                // Set the ContentLength tag
                                response.ContentLength64 = memoryStream.Length;

                                // Copy back to the OutputStream
                                await memoryStream.CopyToAsync(response.OutputStream, cancellationToken);
                                await response.OutputStream.FlushAsync(cancellationToken);
                                response.StatusCode = (int)HttpStatusCode.OK;
                                response.StatusDescription = "OK";
                            }
                        }
                        catch (Exception ex)
                        {
                            var title = "Internal Server Error";
                            var message = WebUtility.HtmlEncode(ex.Message);
                            var stackTrace = WebUtility.HtmlEncode(ex.StackTrace ?? "");

                            var html = $$"""
                                <!DOCTYPE html>
                                <html>
                                <head>
                                    <meta charset="utf-8">
                                    <title>{{title}}</title>
                                    <style>
                                        body {
                                            font-family: sans-serif;
                                            background: #fdfdfd;
                                            color: #333;
                                            padding: 1rem;
                                        }
                                        h1 {
                                            color: #c00;
                                        }
                                        pre {
                                            background: #f5f5f5;
                                            padding: 1rem;
                                            overflow-x: auto;
                                            border: 1px solid #ccc;
                                        }
                                    </style>
                                </head>
                                <body>
                                    <h1>{{title}}</h1>
                                    <p>{{message}}</p>
                                    <pre>{{stackTrace}}</pre>
                                </body>
                                </html>
                            """;

                            try
                            {
                                var buffer = Encoding.UTF8.GetBytes(html);
                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                response.ContentType = "text/html";
                                response.ContentLength64 = buffer.Length;
                                await response.OutputStream.WriteAsync(buffer, cancellationToken);
                            }
                            catch (Exception) { }
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
