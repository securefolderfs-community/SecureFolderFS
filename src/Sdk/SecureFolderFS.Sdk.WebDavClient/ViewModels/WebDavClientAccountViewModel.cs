using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Accounts.DataModels;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Sdk.WebDavClient.DataModels;
using SecureFolderFS.Sdk.WebDavClient.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using WebDav;

namespace SecureFolderFS.Sdk.WebDavClient.ViewModels
{
    [Bindable(true)]
    public sealed partial class WebDavClientAccountViewModel : AccountViewModel
    {
        private IWebDavClient? _webDavClient;
        private HttpClient? _httpClient;
        private Uri? _baseUri;

        [ObservableProperty] private string? _Address;
        [ObservableProperty] private string? _Port;
        [ObservableProperty] private string? _UserName;
        [ObservableProperty] private string? _Password;
        [ObservableProperty] private bool _AcceptFirstCertificate = true;
        [ObservableProperty] private string? _ManualCertificateFingerprint;
        [ObservableProperty] private string? _TrustedCertificateFingerprint;

        /// <inheritdoc/>
        public override string DataSourceType { get; } = Constants.DATA_SOURCE_WEBDAV;

        public WebDavClientAccountViewModel(AccountDataModel dataModel, IPropertyStore<string> propertyStore)
            : base(dataModel, propertyStore)
        {
        }

        public WebDavClientAccountViewModel(string id, IPropertyStore<string> propertyStore, string? title)
            : base(id, propertyStore, title)
        {
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromUserInputAsync(CancellationToken cancellationToken)
        {
            if (_webDavClient is not null && _httpClient is not null && _baseUri is not null)
                return new DavClientFolder(_webDavClient, _httpClient, _baseUri, _baseUri.AbsolutePath, string.Empty);

            return await ConnectAsync(Address, Port, UserName, Password, AcceptFirstCertificate, ManualCertificateFingerprint, TrustedCertificateFingerprint, cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromDataModelAsync(CancellationToken cancellationToken)
        {
            if (_webDavClient is not null && _httpClient is not null && _baseUri is not null)
                return new DavClientFolder(_webDavClient, _httpClient, _baseUri, _baseUri.AbsolutePath, string.Empty);

            ArgumentNullException.ThrowIfNull(propertyStore);
            ArgumentNullException.ThrowIfNull(accountDataModel);

            if (string.IsNullOrEmpty(accountDataModel.AccountId))
                throw new ArgumentException("AccountId cannot be empty.");

            var rawData = await propertyStore.GetValueAsync<string?>(accountDataModel.AccountId, null, cancellationToken);
            if (string.IsNullOrEmpty(rawData))
                throw new ArgumentException("Data cannot be empty.");

            var dataModel = await StreamSerializer.Instance.DeserializeFromStringAsync<WebDavClientAccountDataModel?>(rawData, cancellationToken);
            if (dataModel is null)
                throw new ArgumentException("Data cannot be deserialized.");

            // Update properties from the data model (except password for security reasons)
            Address = dataModel.Address;
            Port = dataModel.Port;
            UserName = dataModel.UserName;
            AcceptFirstCertificate = dataModel.AcceptFirstCertificate;
            ManualCertificateFingerprint = dataModel.ManualCertificateFingerprint;
            TrustedCertificateFingerprint = dataModel.TrustedCertificateFingerprint;

            // Connect using the data model
            return await ConnectAsync(dataModel.Address, dataModel.Port, dataModel.UserName, dataModel.Password, dataModel.AcceptFirstCertificate, dataModel.ManualCertificateFingerprint, dataModel.TrustedCertificateFingerprint, cancellationToken);
        }

        /// <inheritdoc/>
        protected override Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (_webDavClient is null)
                return Task.CompletedTask;

            if (_webDavClient is IDisposable disposableClient)
                disposableClient.Dispose();

            _webDavClient?.Dispose();
            _httpClient?.Dispose();
            _webDavClient = null;
            _httpClient = null;
            _baseUri = null;
            IsConnected = false;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken)
        {
            if (_webDavClient is null || _baseUri is null)
                return false;

            try
            {
                var propfindParams = new PropfindParameters
                {
                    CancellationToken = cancellationToken,
                    Headers = new List<KeyValuePair<string, string>>()
                };

                var response = await _webDavClient.Propfind(_baseUri, propfindParams);
                return response.IsSuccessful;
            }
            catch
            {
                await DisconnectAsync(cancellationToken);
                return false;
            }
        }

        /// <inheritdoc/>
        protected override async Task SaveAccountAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(propertyStore);
            var dataModel = new WebDavClientAccountDataModel(AccountId, DataSourceType, UserName)
            {
                Address = Address,
                Port = Port,
                UserName = UserName,
                Password = Password,
                AcceptFirstCertificate = AcceptFirstCertificate,
                ManualCertificateFingerprint = ManualCertificateFingerprint,
                TrustedCertificateFingerprint = TrustedCertificateFingerprint
            };

            // Serialize and set
            var serialized = await StreamSerializer.Instance.SerializeToStringAsync(dataModel, cancellationToken);
            await propertyStore.SetValueAsync(AccountId, serialized, cancellationToken);
        }

        private async Task<IFolder> ConnectAsync(string? address, string? port, string? username, string? password, bool acceptFirstCertificate, string? manualCertificateFingerprint, string? trustedCertificateFingerprint, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be empty.");

            try
            {
                // Ensure the address has a scheme so UriBuilder parses it correctly.
                // Without "http://" or "https://", UriBuilder misinterprets the host and path.
                var normalizedAddress = !address.Contains("://")
                    ? $"http://{address}"
                    : address;

                // Build the base URI. A port typed inside the address is preserved; the dedicated
                // port value (when provided) takes precedence over it.
                var uriBuilder = new UriBuilder(normalizedAddress);
                if (int.TryParse(port, out var portValue) && portValue > 0)
                    uriBuilder.Port = portValue;

                if (!uriBuilder.Path.EndsWith('/'))
                    uriBuilder.Path += '/';

                _baseUri = uriBuilder.Uri;

                // The timeout must not apply to the whole client: it would abort every later file
                // transfer through this connection after 5 seconds. Only the verification PROPFIND below is capped (see timeoutCts).
                _httpClient = new HttpClient(CreateHandler(acceptFirstCertificate, manualCertificateFingerprint, trustedCertificateFingerprint))
                {
                    BaseAddress = _baseUri,
                    Timeout = Timeout.InfiniteTimeSpan
                };

                if (!string.IsNullOrEmpty(username))
                {
                    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password ?? string.Empty}"));
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                }

                _webDavClient = new WebDav.WebDavClient(_httpClient);

                // Verify the connection by performing a PROPFIND on root (capped so a dead server
                // fails fast without limiting later transfers on the same client)
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));
                var propfindParams = new PropfindParameters
                {
                    CancellationToken = timeoutCts.Token,
                    Headers = new List<KeyValuePair<string, string>>()
                };

                var response = await _webDavClient.Propfind(_baseUri, propfindParams);
                if (!response.IsSuccessful)
                    throw new InvalidOperationException($"Failed to connect to WebDAV server at '{_baseUri}': {response.StatusCode}");

                IsConnected = true;
                return new DavClientFolder(_webDavClient, _httpClient, _baseUri, _baseUri.AbsolutePath, string.Empty);
            }
            catch (Exception)
            {
                await DisconnectAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Creates the HTTP handler for the platform:
        /// <list type="bullet">
        /// <item>Browser (WASM): the default handler, which routes through the fetch API.
        /// <see cref="SocketsHttpHandler"/> is unavailable there (its constructor throws
        /// <see cref="PlatformNotSupportedException"/>), and TLS validation/certificate pinning
        /// is owned by the browser itself.</item>
        /// <item>Everywhere else: <see cref="SocketsHttpHandler"/>, needed for non-standard HTTP
        /// methods (PROPFIND, MKCOL, etc.) - platform-default handlers such as Android's
        /// AndroidMessageHandler reject them - with fingerprint-pinning TLS validation.</item>
        /// </list>
        /// </summary>
        private HttpMessageHandler CreateHandler(bool acceptFirstCertificate, string? manualCertificateFingerprint, string? trustedCertificateFingerprint)
        {
            if (OperatingSystem.IsBrowser())
                return new HttpClientHandler();

            var normalizedManualFingerprint = NormalizeFingerprint(manualCertificateFingerprint);
            var normalizedTrustedFingerprint = NormalizeFingerprint(trustedCertificateFingerprint);

            return new SocketsHttpHandler()
            {
                PreAuthenticate = false,
                SslOptions =
                {
                    RemoteCertificateValidationCallback = (_, certificate, _, sslPolicyErrors) =>
                    {
                        var serverFingerprint = GetCertificateFingerprint(certificate);
                        if (string.IsNullOrEmpty(serverFingerprint))
                            return false;

                        // Manual pinning takes precedence over TOFU/default validation.
                        if (!string.IsNullOrWhiteSpace(normalizedManualFingerprint))
                            return string.Equals(serverFingerprint, normalizedManualFingerprint, StringComparison.OrdinalIgnoreCase);

                        if (!string.IsNullOrWhiteSpace(normalizedTrustedFingerprint))
                            return string.Equals(serverFingerprint, normalizedTrustedFingerprint, StringComparison.OrdinalIgnoreCase);

                        if (acceptFirstCertificate)
                        {
                            normalizedTrustedFingerprint = serverFingerprint;
                            TrustedCertificateFingerprint = serverFingerprint;
                            return true;
                        }

                        return sslPolicyErrors == SslPolicyErrors.None;
                    }
                }
            };
        }

        private void UpdateInputValidation()
        {
            IsInputFilled = !string.IsNullOrEmpty(Address);
        }

        private static string? NormalizeFingerprint(string? fingerprint)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
                return null;

            return fingerprint.Replace(":", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToUpperInvariant();
        }

        private static string? GetCertificateFingerprint(X509Certificate? certificate)
        {
            if (certificate is null)
                return null;

            var cert2 = certificate as X509Certificate2 ?? new X509Certificate2(certificate);
            var hash = cert2.GetCertHashString(System.Security.Cryptography.HashAlgorithmName.SHA256);
            return NormalizeFingerprint(hash);
        }

        partial void OnAddressChanged(string? value)
        {
            _ = value;
            UpdateInputValidation();
        }

        partial void OnPortChanged(string? value)
        {
            _ = value;
            UpdateInputValidation();
        }

        partial void OnUserNameChanged(string? value)
        {
            _ = value;
            UpdateInputValidation();
        }

        partial void OnManualCertificateFingerprintChanged(string? value)
        {
            _ = value;
            UpdateInputValidation();
        }

        partial void OnAcceptFirstCertificateChanged(bool value)
        {
            _ = value;
            UpdateInputValidation();
        }
    }
}
