using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dropbox.Api;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Accounts.DataModels;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Sdk.Dropbox.DataModels;
using SecureFolderFS.Sdk.Dropbox.Storage;
using SecureFolderFS.Shared.Api;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.Dropbox.ViewModels
{
    [Bindable(true)]
    public sealed partial class DropboxAccountViewModel : AccountViewModel
    {
        private readonly IOAuthHandler _oauthHandler;
        private DropboxClient? _client;
        private string? _accessToken;
        private string? _refreshToken;
        private DateTime? _expiresAt;

        [ObservableProperty] private string? _UserDisplayName;
        [ObservableProperty] private string? _UserEmail;
        [ObservableProperty] private Uri? _UserPhotoUri;

        /// <inheritdoc/>
        public override string DataSourceType { get; } = Constants.DATA_SOURCE_DROPBOX;

        public DropboxAccountViewModel(AccountDataModel accountDataModel, IPropertyStore<string> propertyStore, IOAuthHandler oauthHandler)
            : base(accountDataModel, propertyStore)
        {
            _oauthHandler = oauthHandler;
        }

        public DropboxAccountViewModel(string accountId, IPropertyStore<string> propertyStore, string? title, IOAuthHandler oauthHandler)
            : base(accountId, propertyStore, title)
        {
            _oauthHandler = oauthHandler;
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromUserInputAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();

            if (_client is not null)
                return GetRootDropboxFolder(_client);

            (_client, _accessToken, _refreshToken, _expiresAt) = await AuthorizeAsync(cancellationToken);

            await FetchUserInfoAsync();
            IsInputFilled = IsConnected = true;

            await SaveAccountAsync(cancellationToken);
            return GetRootDropboxFolder(_client);
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromDataModelAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();

            if (_client is not null)
                return GetRootDropboxFolder(_client);

            ArgumentNullException.ThrowIfNull(propertyStore);
            ArgumentNullException.ThrowIfNull(accountDataModel);

            if (string.IsNullOrEmpty(accountDataModel.AccountId))
                throw new ArgumentException("AccountId cannot be empty.");

            var rawData = await propertyStore.GetValueAsync<string?>(accountDataModel.AccountId, null, cancellationToken);
            if (string.IsNullOrEmpty(rawData))
                throw new ArgumentException("Stored data cannot be empty.");

            var dataModel = await StreamSerializer.Instance.DeserializeFromStringAsync<DropboxAccountDataModel?>(rawData, cancellationToken);
            if (dataModel is null)
                throw new ArgumentException("Stored data could not be deserialized.");

            // Restore display name optimistically while we verify the connection
            UserDisplayName = dataModel.DisplayName;

            if (string.IsNullOrEmpty(dataModel.RefreshToken))
            {
                // No refresh token stored — full re-authorization required
                (_client, _accessToken, _refreshToken, _expiresAt) = await AuthorizeAsync(cancellationToken);
            }
            else
            {
                try
                {
                    // Attempt silent restore using the stored tokens
                    _accessToken = dataModel.AccessToken;
                    _refreshToken = dataModel.RefreshToken;
                    _expiresAt = dataModel.ExpiresAt;
                    _client = BuildClient(_accessToken!, _refreshToken!, _expiresAt);

                    // Validates the token is still accepted
                    await FetchUserInfoAsync();
                }
                catch (AuthException)
                {
                    // Token was revoked or otherwise invalid — re-authorize
                    _client?.Dispose();
                    (_client, _accessToken, _refreshToken, _expiresAt) = await AuthorizeAsync(cancellationToken);
                }
            }

            IsInputFilled = IsConnected = true;
            await FetchUserInfoAsync();
            await SaveAccountAsync(cancellationToken);

            return GetRootDropboxFolder(_client);
        }

        /// <inheritdoc/>
        [RelayCommand]
        protected override async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (_client is null)
                return;

            _client.Dispose();
            _client = null;
            _accessToken = null;
            _refreshToken = null;
            _expiresAt = null;
            IsInputFilled = IsConnected = false;
            UserDisplayName = null;
            UserPhotoUri = null;
            UserEmail = null;

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken)
        {
            if (_client is null)
                return false;

            try
            {
                // Lightweight call to verify the token is still valid
                var account = await _client.Users.GetCurrentAccountAsync();
                return account is not null;
            }
            catch (AuthException)
            {
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
            ArgumentNullException.ThrowIfNull(propertyStore);
            ArgumentNullException.ThrowIfNull(_refreshToken);

            var dataModel = new DropboxAccountDataModel(AccountId, DataSourceType, UserDisplayName)
            {
                AccessToken = _accessToken,
                RefreshToken = _refreshToken,
                ExpiresAt = _expiresAt
            };

            var serialized = await StreamSerializer.Instance.SerializeToStringAsync(dataModel, cancellationToken);
            await propertyStore.SetValueAsync(AccountId, serialized, cancellationToken);
        }

        [RelayCommand]
        private async Task ConnectAccountPromptAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ConnectFromUserInputAsync(cancellationToken);
            }
            catch (Exception)
            {
                UserDisplayName = null;
                UserPhotoUri = null;
                UserEmail = null;
            }
        }

        /// <summary>
        /// Runs the PKCE authorization flow via the platform <see cref="IOAuthHandler"/>,
        /// exchanges the code for tokens, and returns a ready <see cref="DropboxClient"/>.
        /// </summary>
        private async Task<(DropboxClient, string, string, DateTime?)> AuthorizeAsync(
            CancellationToken cancellationToken)
        {
            AssertApiAccess();

            var pkce = new PKCEOAuthFlow();
            var state = Guid.NewGuid().ToString("N");

            // Build the authorization URI — IOAuthHandler supplies the redirect URL
            var authorizeUri = pkce.GetAuthorizeUri(
                oauthResponseType: OAuthResponseType.Code,
                clientId: ApiKeys.DropboxAppKey,
                redirectUri: Constants.DROPBOX_REDIRECT_URI,
                state: state,
                tokenAccessType: TokenAccessType.Offline);

            // Delegate browser opening + code capture to the platform handler
            var result = await _oauthHandler.GetCodeAsync(authorizeUri.ToString(), Constants.DROPBOX_REDIRECT_URI, cancellationToken);
            if (!result.Successful)
                throw new InvalidOperationException($"Dropbox authorization failed: {result.Value?.Error}");

            var oauthResult = result.Value!;

            // Guard against CSRF — verify the returned state matches what we sent
            if (oauthResult.State?.ToString() != state)
                throw new InvalidOperationException(
                    "OAuth state mismatch. The authorization response may have been tampered with.");

            var code = oauthResult.Code?.ToString()
                       ?? throw new InvalidOperationException("Dropbox did not return an authorization code.");

            // Exchange the code for an access + refresh token pair
            var tokenResponse = await pkce.ProcessCodeFlowAsync(
                code: code,
                appKey: ApiKeys.DropboxAppKey,
                redirectUri: Constants.DROPBOX_REDIRECT_URI);

            return (BuildClient(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.ExpiresAt),
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                tokenResponse.ExpiresAt);
        }

        /// <summary>
        /// Fetches the current user's display name, email, and photo from the Dropbox API.
        /// </summary>
        private async Task FetchUserInfoAsync()
        {
            if (_client is null)
                throw new InvalidOperationException("Dropbox client is not initialized.");

            try
            {
                var account = await _client.Users.GetCurrentAccountAsync();
                UserDisplayName = account.Name.DisplayName;
                UserEmail = account.Email;
                UserPhotoUri = !string.IsNullOrEmpty(account.ProfilePhotoUrl)
                    ? new Uri(account.ProfilePhotoUrl)
                    : null;
            }
            catch (AuthException)
            {
                // Surface auth errors so callers can trigger re-authorization
                throw;
            }
            catch (Exception)
            {
                // Non-fatal: keep whatever display name we had
                UserDisplayName ??= "Unknown user";
                UserEmail = null;
            }
        }

        private static DropboxClient BuildClient(string accessToken, string refreshToken, DateTime? expiresAt)
        {
            return new(
                oauth2AccessToken: accessToken,
                oauth2RefreshToken: refreshToken,
                oauth2AccessTokenExpiresAt: expiresAt ?? DateTime.UtcNow,
                appKey: ApiKeys.DropboxAppKey);
        }

        private static IFolder GetRootDropboxFolder(DropboxClient client)
        {
            return new DropboxFolder(client, string.Empty, "Dropbox");
        }

        public static void AssertApiAccess()
        {
            if (string.IsNullOrEmpty(ApiKeys.DropboxAppKey))
                throw new InvalidOperationException("Dropbox app key is not set.");
        }
    }
}