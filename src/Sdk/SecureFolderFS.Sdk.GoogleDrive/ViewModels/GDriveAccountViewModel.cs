using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Accounts.DataModels;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Sdk.GoogleDrive.AppModels;
using SecureFolderFS.Sdk.GoogleDrive.DataModels;
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
        private readonly ICodeReceiver _codeReceiver;
        private readonly IDataStore _propertyDataStore;
        private DriveService? _driveService;
        private UserCredential? _credential;
        private string? _userId;

        [ObservableProperty] private string? _UserDisplayName;
        [ObservableProperty] private string? _UserEmail;
        [ObservableProperty] private Uri? _UserPhotoUri;

        /// <inheritdoc/>
        public override string DataSourceType { get; } = Constants.DATA_SOURCE_GOOGLE_DRIVE;

        public GDriveAccountViewModel(AccountDataModel accountDataModel, IPropertyStore<string> propertyStore, IOAuthHandler authHandler)
            : base(accountDataModel, propertyStore)
        {
            _codeReceiver = new OAuthCodeReceiver(authHandler);
            _propertyDataStore = new PropertyDataStore(propertyStore);
        }

        public GDriveAccountViewModel(string accountId, IPropertyStore<string> propertyStore, string? title, IOAuthHandler authHandler)
            : base(accountId, propertyStore, title)
        {
            _codeReceiver = new OAuthCodeReceiver(authHandler);
            _propertyDataStore = new PropertyDataStore(propertyStore);
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromUserInputAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();

            if (_driveService is not null)
                return GetRootFolder(_driveService);

            _userId = Guid.NewGuid().ToString();
            _credential = await AuthorizeAsync(_userId, forcePrompt: true, cancellationToken: cancellationToken);
            _driveService = CreateDriveService(_credential);

            // Fetch user information
            await FetchUserInfoAsync(cancellationToken);
            IsInputFilled = IsConnected = true;

            await SaveAccountAsync(cancellationToken);
            return GetRootFolder(_driveService);
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromDataModelAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();

            if (_driveService is not null)
                return GetRootFolder(_driveService);

            ArgumentNullException.ThrowIfNull(propertyStore);
            ArgumentNullException.ThrowIfNull(accountDataModel);

            if (string.IsNullOrEmpty(accountDataModel.AccountId))
                throw new ArgumentException("AccountId cannot be empty.");

            var rawData = await propertyStore.GetValueAsync<string?>(accountDataModel.AccountId, null, cancellationToken);
            if (string.IsNullOrEmpty(rawData))
                throw new ArgumentException("Data cannot be empty.");

            var gdriveAccountDataModel = await StreamSerializer.Instance.DeserializeFromStringAsync<GDriveAccountDataModel?>(rawData, cancellationToken);
            if (gdriveAccountDataModel is null)
                throw new ArgumentException("Data cannot be deserialized.");

            // Load stored user info
            UserDisplayName = gdriveAccountDataModel.DisplayName;

            // Attempt silent authentication with stored credentials
            var userId = gdriveAccountDataModel.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                // No userId stored, force reauthorization
                userId = Guid.NewGuid().ToString();
                _userId = userId;
                _credential = await AuthorizeAsync(userId, forcePrompt: true, cancellationToken);
            }
            else
            {
                _userId = userId;
                try
                {
                    // Try silent authentication
                    _credential = await AuthorizeAsync(userId, forcePrompt: false, cancellationToken);
                }
                catch (Exception)
                {
                    // Credentials are missing or invalid, force reauthorization
                    _userId = Guid.NewGuid().ToString();
                    _credential = await AuthorizeAsync(_userId, forcePrompt: true, cancellationToken);
                }
            }

            _driveService = CreateDriveService(_credential);

            // Verify connection and refresh user info
            try
            {
                await FetchUserInfoAsync(cancellationToken);
                IsInputFilled = IsConnected = true;

                // Update saved data if userId changed
                if (_userId != userId)
                    await SaveAccountAsync(cancellationToken);
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
            {
                // Token expired, force reauthorization
                await DisconnectAsync(cancellationToken);
                _userId = Guid.NewGuid().ToString();
                _credential = await AuthorizeAsync(_userId, forcePrompt: true, cancellationToken);
                _driveService = CreateDriveService(_credential);
                await FetchUserInfoAsync(cancellationToken);
                IsInputFilled = IsConnected = true;
                await SaveAccountAsync(cancellationToken);
            }

            return GetRootFolder(_driveService);
        }

        /// <inheritdoc/>
        [RelayCommand]
        protected override async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (_driveService is null)
                return;

            _driveService.Dispose();
            _driveService = null;
            _credential = null;
            _userId = null;
            IsInputFilled = IsConnected = false;
            UserDisplayName = null;
            UserPhotoUri = null;
            UserEmail = null;

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
                aboutRequest.Fields = "user"; // We're checking for the "user" field

                var about = await aboutRequest.ExecuteAsync(cancellationToken);
                return about is not null;
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
            {
                // Token expired or revoked
                await DisconnectAsync(cancellationToken);
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override async Task SaveAccountAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_userId);
            ArgumentNullException.ThrowIfNull(propertyStore);

            // Store basic account info
            var dataModel = new GDriveAccountDataModel(AccountId, DataSourceType, UserDisplayName)
            {
                UserId = _userId
            };

            // Serialize and set
            var serialized = await StreamSerializer.Instance.SerializeToStringAsync(dataModel, cancellationToken);
            await propertyStore.SetValueAsync(AccountId, serialized, cancellationToken);
        }

        [RelayCommand]
        private async Task ConnectAccountPromptAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ConnectFromUserInputAsync(cancellationToken);
                // User info is already fetched in ConnectFromUserInputAsync
            }
            catch (Exception)
            {
                UserDisplayName = null;
                UserPhotoUri = null;
                UserEmail = null;
            }
        }

        /// <summary>
        /// Fetches user information from Google Drive API and updates the view model properties.
        /// </summary>
        private async Task FetchUserInfoAsync(CancellationToken cancellationToken)
        {
            if (_driveService is null)
                throw new InvalidOperationException("Drive service is not initialized.");

            try
            {
                var aboutRequest = _driveService.About.Get();
                aboutRequest.Fields = "user(displayName,emailAddress,photoLink)";

                var about = await aboutRequest.ExecuteAsync(cancellationToken);
                if (about?.User is not null)
                {
                    UserDisplayName = about.User.DisplayName;
                    UserPhotoUri = !string.IsNullOrEmpty(about.User.PhotoLink) ? new(about.User.PhotoLink) : null;
                    UserEmail = about.User.EmailAddress;
                }
            }
            catch (Exception ex)
            {
                // TODO: Log or handle the error appropriately
                // For now, just set default values
                UserDisplayName = _credential?.UserId ?? "Unknown user";
                UserEmail = null;

                // Re-throw if it's a critical error
                if (ex is GoogleApiException apiEx && apiEx.HttpStatusCode == HttpStatusCode.Unauthorized)
                    throw;
            }
        }

        private static DriveService CreateDriveService(UserCredential credential)
        {
            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = nameof(SecureFolderFS),
            });
        }

        private async Task<UserCredential> AuthorizeAsync(string userId, bool forcePrompt = false, CancellationToken cancellationToken = default)
        {
            AssertApiAccess();

            // Load client secrets
            using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(ApiKeys.GoogleDriveClientKey);
            await writer.FlushAsync(cancellationToken);
            stream.Position = 0;

            // If forcePrompt, clear only this user's stored token to re-prompt authorization
            if (forcePrompt)
            {
                // We need to delete the specific token for this user only
                await _propertyDataStore.DeleteAsync<object>(userId);
            }

            var clientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets.Secrets,
                [ DriveService.Scope.Drive ],
                userId,
                cancellationToken,
                _propertyDataStore,
                _codeReceiver);

            return credential;
        }

        private IFolder GetRootFolder(DriveService driveService)
        {
            return new GDriveFolder(driveService, "root", "Drive");
        }

        public static void AssertApiAccess()
        {
            if (string.IsNullOrEmpty(ApiKeys.GoogleDriveClientKey))
                throw new InvalidOperationException("Google Drive API key is not set.");
        }
    }
}