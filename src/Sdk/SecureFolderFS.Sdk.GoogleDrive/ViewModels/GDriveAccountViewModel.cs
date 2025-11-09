using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Accounts.DataModels;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Sdk.GoogleDrive.Storage;
using SecureFolderFS.Shared.Api;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.GoogleDrive.ViewModels
{
    [Bindable(true)]
    public sealed partial class GDriveAccountViewModel : AccountViewModel
    {
        private DriveService? _driveService;
        private UserCredential? _credential;

        public override string DataSourceType { get; } = Constants.DATA_SOURCE_GOOGLE_DRIVE;

        public GDriveAccountViewModel(AccountDataModel accountDataModel, IPropertyStore<string> propertyStore)
            : base(accountDataModel, propertyStore)
        {
        }

        public GDriveAccountViewModel(string accountId, IPropertyStore<string> propertyStore, string? title)
            : base(accountId, propertyStore, title)
        {
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromUserInputAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();

            if (_driveService is not null)
                return new GDriveFolder(_driveService, "/", string.Empty);

            _credential = await AuthorizeAsync(cancellationToken);
            _driveService = CreateDriveService(_credential);
            IsConnected = true;

            await SaveAccountAsync(cancellationToken);
            return new GDriveFolder(_driveService, "/", string.Empty);
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromDataModelAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();

            if (_driveService is not null)
                return new GDriveFolder(_driveService, "/", string.Empty);

            // Attempt to reuse existing credentials stored by PropertyDataStore
            try
            {
                _credential = await AuthorizeAsync(cancellationToken);
                _driveService = CreateDriveService(_credential);
                IsConnected = true;
            }
            catch (Exception)
            {
                // If credentials are invalid, force user reauthorization
                _credential = await AuthorizeAsync(cancellationToken, forcePrompt: true);
                _driveService = CreateDriveService(_credential);
                IsConnected = true;
            }

            return new GDriveFolder(_driveService, "/", string.Empty);
        }

        /// <inheritdoc/>
        protected override async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (_driveService is null)
                return;

            _driveService.Dispose();
            _driveService = null;
            _credential = null;
            IsConnected = false;

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken)
        {
            if (_driveService is null)
                return false;

            try
            {
                // Simple metadata request to test connection
                var aboutRequest = _driveService.About.Get();
                aboutRequest.Fields = "user";
                var about = await aboutRequest.ExecuteAsync(cancellationToken);
                return about is not null;
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
            {
                // Token expired or revoked
                IsConnected = false;
                await DisconnectAsync(cancellationToken);
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override async Task SaveAccountAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(propertyStore);

            // Store basic account info
            var dataModel = new AccountDataModel(AccountId, DataSourceType, Title);
            var serialized = await StreamSerializer.Instance.SerializeToStringAsync(dataModel, cancellationToken);

            await propertyStore.SetValueAsync(AccountId, serialized, cancellationToken);
        }

        private static DriveService CreateDriveService(UserCredential credential)
        {
            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = nameof(SecureFolderFS),
            });
        }

        private async Task<UserCredential> AuthorizeAsync(CancellationToken cancellationToken, bool forcePrompt = false)
        {
            AssertApiAccess();
            var apiKey = ApiKeys.GoogleDriveClientKey;

            // Load client secrets
            using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(apiKey);
            await writer.FlushAsync(cancellationToken);
            stream.Position = 0;

            var secrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);
            var scopes = new[] { DriveService.Scope.Drive };

            // If forcePrompt, clear stored token to re-prompt authorization
            var dataStore = new PropertyDataStore(propertyStore);
            if (forcePrompt)
                await dataStore.DeleteAsync<object>("user");

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets.Secrets,
                scopes,
                "user",
                cancellationToken,
                dataStore);

            return credential;
        }

        public static void AssertApiAccess()
        {
            if (string.IsNullOrEmpty(ApiKeys.GoogleDriveClientKey))
                throw new InvalidOperationException("Google Drive API key is not set.");
        }
    }
}
