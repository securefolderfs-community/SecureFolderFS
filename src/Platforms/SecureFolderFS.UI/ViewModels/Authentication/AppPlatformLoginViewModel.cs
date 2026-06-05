using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;
#if APP_PLATFORM_PRESENT
using SecureFolderFS.Sdk.AppPlatform;
#endif

namespace SecureFolderFS.UI.ViewModels.Authentication
{
    /// <summary>
    /// The system browser authenticates via Keycloak, decrypts the vault key
    /// client-side, and passes the result to a localhost callback.
    /// </summary>
    public sealed partial class AppPlatformLoginViewModel : AuthenticationViewModel
    {
        private readonly IFolder _vaultFolder;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        public override bool CanComplement { get; } = false;

        /// <inheritdoc/>
        public override AuthenticationStage Availability { get; } = AuthenticationStage.FirstStageOnly;

        public AppPlatformLoginViewModel(IFolder vaultFolder)
            : base(Core.Constants.Vault.Authentication.AUTH_APP_PLATFORM)
        {
            _vaultFolder = vaultFolder;
            Title = "App Platform";
        }

        /// <inheritdoc/>
        public override Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            return Task.FromException(new NotSupportedException());
        }

        /// <inheritdoc/>
        public override Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IResult<IKeyBytes>>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public override Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IResult<IKeyBytes>>(new NotSupportedException());
        }

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
#if APP_PLATFORM_PRESENT
            if (!OperatingSystem.IsBrowser())
            {
                await ProvideCredentialsNativeAsync(cancellationToken);
                return;
            }
#endif
            await ProvideCredentialsViaBrowserAsync(cancellationToken);
        }

#if APP_PLATFORM_PRESENT
        /// <summary>
        /// Native desktop flow: OIDC auth via system browser, then decrypt the vault key
        /// entirely in .NET (no JS interop, no jose library mismatch).
        /// If this is the first unlock on this device, prompts for the Account Key passphrase
        /// to bootstrap the device key chain.
        /// </summary>
        private async Task ProvideCredentialsNativeAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(_vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);

            if (config.AppPlatform is null)
                throw new InvalidOperationException("Vault is not configured for App Platform.");

            var serverUrl = config.AppPlatform.ServerUrl.TrimEnd('/');
            var vaultId = config.Uid;

            var authProvider = DI.Service<IOidcProvider>();
            var deviceKeyStore = DI.Service<IDeviceKeyStore>();

            using var client = new AppPlatformClient(serverUrl);
            var authConfig = await client.GetAuthConfigAsync(cancellationToken);
            var accessToken = await authProvider.GetAccessTokenAsync(
                authConfig.Authority, authConfig.ClientId, authConfig.Scopes, cancellationToken);
            client.SetAccessToken(accessToken);

            var keyManager = new AppPlatformKeyManager(deviceKeyStore, client, authProvider);

            // If no device is registered on this machine, bootstrap one using the Account Key passphrase
            if (!await deviceKeyStore.HasPrivateKeyAsync(cancellationToken))
            {
                StateChanged?.Invoke(this, EventArgs.Empty);

                var overlayService = DI.Service<IOverlayService>();
                var overlay = new DeviceSetupOverlayViewModel();
                var result = await overlayService.ShowAsync(overlay);

                if (!result.Successful || string.IsNullOrEmpty(overlay.Passphrase))
                    throw new OperationCanceledException("Account Key passphrase is required to set up this device.");

                var deviceName = Environment.MachineName;
                await keyManager.BootstrapDeviceAsync(deviceName, overlay.Passphrase, cancellationToken);
            }

            var (dekKey, macKey) = await keyManager.DecryptVaultKeyAsync(vaultId, cancellationToken);

            var combined = new byte[dekKey.Length + macKey.Length];
            try
            {
                Array.Copy(dekKey, 0, combined, 0, dekKey.Length);
                Array.Copy(macKey, 0, combined, dekKey.Length, macKey.Length);

                using var key = ManagedKey.TakeOwnership(combined);
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new(key, tcs));
                await tcs.Task;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(dekKey);
                CryptographicOperations.ZeroMemory(macKey);
            }
        }
#endif

        /// <summary>
        /// Browser-based fallback: opens the server's unlock page which handles authentication
        /// and vault key decryption in-browser, then POSTs the decrypted key back to localhost.
        /// </summary>
        private async Task ProvideCredentialsViaBrowserAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(_vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);

            if (config.AppPlatform is null)
                throw new InvalidOperationException("Vault is not configured for App Platform.");

            var serverUrl = config.AppPlatform.ServerUrl.TrimEnd('/');
            var vaultId = config.Uid;

            // Find a free localhost port for the callback
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();

            var callbackUri = $"http://localhost:{port}/";

            // The server renders a page that authenticates via Keycloak, decrypts the
            // vault key in-browser using the user's private key (Web Crypto / IndexedDB),
            // and redirects the decrypted key to our callback.
            var unlockUrl = $"{serverUrl}/app/unlock" +
                            $"?vault={Uri.EscapeDataString(vaultId)}" +
                            $"&redirect={Uri.EscapeDataString(callbackUri)}";

            using var httpListener = new HttpListener();
            httpListener.Prefixes.Add(callbackUri);
            httpListener.Start();

            try
            {
                Process.Start(new ProcessStartInfo(unlockUrl) { UseShellExecute = true });

                var context = await httpListener.GetContextAsync().WaitAsync(cancellationToken);

                // The unlock page POSTs the key in the request body (not the URL)
                // to avoid leaking decryption keys in browser history / Referer headers.
                // Error callbacks use GET with ?error= (no sensitive data).
                byte[] combined;
                if (context.Request.HttpMethod == "POST")
                {
                    using var reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                    var body = await reader.ReadToEndAsync(cancellationToken);
                    var formData = HttpUtility.ParseQueryString(body);
                    var keyParam = formData.Get("key");

                    if (string.IsNullOrEmpty(keyParam))
                    {
                        SendHtmlResponse(context, false, "No key in POST body.");
                        throw new InvalidOperationException("App Platform unlock failed: no key received.");
                    }

                    SendHtmlResponse(context, true, null);
                    combined = Base64UrlDecode(keyParam);
                }
                else
                {
                    var queryParams = HttpUtility.ParseQueryString(context.Request.Url?.Query ?? string.Empty);
                    var errorParam = queryParams.Get("error") ?? "Unknown error";
                    SendHtmlResponse(context, false, errorParam);
                    throw new InvalidOperationException($"App Platform unlock failed: {errorParam}");
                }
                try
                {
                    using var key = ManagedKey.TakeOwnership(combined);

                    var tcs = new TaskCompletionSource();
                    CredentialsProvided?.Invoke(this, new(key, tcs));
                    await tcs.Task;
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(combined);
                }
            }
            finally
            {
                httpListener.Stop();
            }
        }

        private static byte[] Base64UrlDecode(string base64Url)
        {
            var s = base64Url.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }

        private static void SendHtmlResponse(HttpListenerContext context, bool success, string? errorMessage)
        {
            var html = success
                ? "<html><body><h2>Vault unlocked</h2><p>You can close this window.</p></body></html>"
                : $"<html><body><h2>Unlock failed</h2><p>{WebUtility.HtmlEncode(errorMessage)}</p></body></html>";

            var buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
    }
}
