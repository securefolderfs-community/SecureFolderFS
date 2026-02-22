using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="IOAuthHandler"/>
    internal sealed class MauiOAuthHandler : IOAuthHandler
    {
        public static IOAuthHandler Instance { get; } = new MauiOAuthHandler();

        private int _port;
        private HttpListener? _httpListener;
        private readonly SemaphoreSlim _portSemaphore = new(1, 1);

        /// <inheritdoc/>
        public string RedirectUrl
        {
            get
            {
                if (_port == 0)
                {
                    _portSemaphore.Wait();
                    try
                    {
                        if (_port == 0)
                        {
                            var listener = new TcpListener(IPAddress.Loopback, 0);
                            listener.Start();

                            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                            listener.Stop();

                            _port = port;
                        }
                    }
                    finally
                    {
                        _portSemaphore.Release();
                    }
                }

                return $"http://localhost:{_port}/";
            }
        }

        private MauiOAuthHandler()
        {
        }

        /// <inheritdoc/>
        public async Task<IResult<OAuthResult>> GetCodeAsync(string url, CancellationToken cancellationToken = default)
        {
            // Ensure the port is set (this will trigger the RedirectUri getter)
            var redirectUri = RedirectUrl;

            // Start HTTP listener on localhost
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(redirectUri);
            _httpListener.Start();

            try
            {
                // Launch the system browser
                await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);

                // Wait for the callback
                var context = await _httpListener.GetContextAsync();

                // Extract query parameters from the callback
                var queryParams = HttpUtility.ParseQueryString(context.Request.Url?.Query ?? string.Empty);

                // Prepare response based on result
                var response = context.Response;
                string responseString;

                var code = queryParams.Get("code");
                var state = queryParams.Get("state");
                var error = queryParams.Get("error");

                // Prepare a authorization result webpage
                if (code is null)
                {
                    await using var stream = await FileSystem.OpenAppPackageFileAsync("auth_fail.html");
                    using var reader = new StreamReader(stream);

                    var errorString = error;
                    errorString ??= "The returned code is empty.";
                    responseString = await reader.ReadToEndAsync(cancellationToken);
                    responseString = responseString.Replace("ERROR_MESSAGE", HttpUtility.HtmlEncode(errorString));
                }
                else
                {
                    await using var stream = await FileSystem.OpenAppPackageFileAsync("auth_success.html");
                    using var reader = new StreamReader(stream);

                    responseString = await reader.ReadToEndAsync(cancellationToken);
                }

                // Send response to the browser
                var buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.ContentType = "text/html";

                // Write and close
                await response.OutputStream.WriteAsync(buffer, cancellationToken);
                response.OutputStream.Close();

                // Return the appropriate response
                return code is not null
                    ? Result<OAuthResult>.Success(new OAuthResult(code, state, null))
                    : Result<OAuthResult>.Failure(new OAuthResult(null, state, error));
            }
            catch (Exception ex)
            {
                return Result<OAuthResult>.Failure(new OAuthResult(null, null, ex.Message), ex);
            }
            finally
            {
                _httpListener?.Stop();
                _httpListener?.Close();
            }
        }
    }
}
