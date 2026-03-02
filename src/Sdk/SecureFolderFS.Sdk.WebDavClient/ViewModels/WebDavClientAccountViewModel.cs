using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
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
            if (_webDavClient is not null && _baseUri is not null)
                return new DavClientFolder(_webDavClient, _baseUri, "/", string.Empty);

            return await ConnectAsync(Address, Port, UserName, Password, cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromDataModelAsync(CancellationToken cancellationToken)
        {
            if (_webDavClient is not null && _baseUri is not null)
                return new DavClientFolder(_webDavClient, _baseUri, "/", string.Empty);

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

            // Connect using the data model
            return await ConnectAsync(dataModel.Address, dataModel.Port, dataModel.UserName, dataModel.Password, cancellationToken);
        }

        /// <inheritdoc/>
        protected override Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (_webDavClient is null)
                return Task.CompletedTask;

            if (_webDavClient is IDisposable disposableClient)
                disposableClient.Dispose();

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
                Password = Password
            };

            // Serialize and set
            var serialized = await StreamSerializer.Instance.SerializeToStringAsync(dataModel, cancellationToken);
            await propertyStore.SetValueAsync(AccountId, serialized, cancellationToken);
        }

        private async Task<IFolder> ConnectAsync(string? address, string? port, string? username, string? password, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be empty.");

            try
            {
                // Build the base URI with optional custom port
                var uriBuilder = new UriBuilder(address);
                if (int.TryParse(port, out var portValue) && portValue > 0)
                    uriBuilder.Port = portValue;

                _baseUri = uriBuilder.Uri;

                // Configure HttpClient with optional authentication
                var handler = new HttpClientHandler();
                if (!string.IsNullOrEmpty(username))
                {
                    handler.Credentials = new NetworkCredential(username, password ?? string.Empty);
                    handler.PreAuthenticate = true;
                }

                _httpClient = new HttpClient(handler)
                {
                    BaseAddress = _baseUri,
                    Timeout = TimeSpan.FromSeconds(30)
                };

                _webDavClient = new WebDav.WebDavClient(_httpClient);

                // Verify the connection by performing a PROPFIND on root
                var propfindParams = new PropfindParameters
                {
                    CancellationToken = cancellationToken,
                    Headers = new List<KeyValuePair<string, string>>()
                };

                var response = await _webDavClient.Propfind(_baseUri, propfindParams);
                if (!response.IsSuccessful)
                    throw new InvalidOperationException($"Failed to connect to WebDAV server at '{_baseUri}': {response.StatusCode}");

                IsConnected = true;
                Password = null;

                return new DavClientFolder(_webDavClient, _baseUri, "/", string.Empty);
            }
            catch (Exception)
            {
                await DisconnectAsync(cancellationToken);
                throw;
            }
        }

        private void UpdateInputValidation()
        {
            IsInputFilled = !string.IsNullOrEmpty(Address);
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
    }
}
