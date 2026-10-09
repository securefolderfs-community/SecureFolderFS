using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using SecureFolderFS.Shared.ComponentModel;

#if ANDROID
using Android.Content.Res;
using SecureFolderFS.Shared.Helpers;
#endif

namespace SecureFolderFS.Maui.AppModels
{
    internal sealed class PdfStreamServer : IAsyncInitialize, IDisposable
    {
        private readonly HttpListener _httpListener;
        private readonly Stream _fileStream;
        private readonly string _mimeType;
        private readonly int _port;
        private readonly string _accessToken;
        private readonly SemaphoreSlim _requestSemaphore;
        private bool _disposed;

        public string BaseAddress => $"http://localhost:{_port}/{_accessToken}";

        public PdfStreamServer(Stream fileStream, string mimeType)
        {
            if (!fileStream.CanSeek)
                throw new ArgumentException("Stream must be seekable.", nameof(fileStream));

            _fileStream = fileStream;
            _mimeType = mimeType;

            // The listener is reachable by every process on the device. Require a
            // cryptographically random token in the path so other local apps cannot
            // read the decrypted document while the preview is open
            _accessToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

            // Requests share one seekable stream so serve them one at a time
            _requestSemaphore = new SemaphoreSlim(1, 1);

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
                        await _requestSemaphore.WaitAsync(cancellationToken);
                        try
                        {
                            await ProcessRequestAsync(context, cancellationToken);
                        }
                        catch (Exception)
                        {
                            TryWriteErrorResponse(context.Response);
                        }
                        finally
                        {
                            _requestSemaphore.Release();
                            try
                            {
                                context.Response.Close();
                            }
                            catch (Exception)
                            {
                                // The connection may already be gone
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = ex;
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            var response = context.Response;
            var absolutePath = context.Request.Url?.AbsolutePath ?? string.Empty;

            // Reject any request that does not carry the access token
            var tokenPrefix = $"/{_accessToken}";
            if (!absolutePath.StartsWith(tokenPrefix, StringComparison.Ordinal))
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            var relativePath = absolutePath[tokenPrefix.Length..];
            if (relativePath == "/app_file")
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = _mimeType;
                response.ContentLength64 = _fileStream.Length;

                _fileStream.Position = 0L;
                await _fileStream.CopyToAsync(response.OutputStream, cancellationToken);
                _fileStream.Position = 0L;
            }
            else if (relativePath.StartsWith("/pdfjs53", StringComparison.Ordinal))
            {
#if ANDROID
                var assetPath = relativePath.TrimStart('/');
                var contentType = FileTypeHelper.GetMimeType(assetPath);

                await using var assetStream = Android.App.Application.Context.Assets?.Open(assetPath, Access.Random);
                if (assetStream is null)
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                // Buffer the asset first - ContentLength64 must be known before the body is written
                await using var memoryStream = new MemoryStream();
                await assetStream.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0L;

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = contentType;
                response.ContentLength64 = memoryStream.Length;

                await memoryStream.CopyToAsync(response.OutputStream, cancellationToken);
                await response.OutputStream.FlushAsync(cancellationToken);
#else
                response.StatusCode = (int)HttpStatusCode.NotFound;
#endif
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        private static void TryWriteErrorResponse(HttpListenerResponse response)
        {
            try
            {
                // Don't leak exception details to other local processes by making it deliberately generic
                var buffer = "Internal Server Error"u8.ToArray();
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.ContentType = "text/plain";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer);
            }
            catch (Exception)
            {
                // Headers may already have been sent
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
            _fileStream.Dispose();
            _httpListener.Abort();
            _requestSemaphore.Dispose();
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
