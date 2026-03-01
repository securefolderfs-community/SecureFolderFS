using System.Text.Json.Serialization;
using SecureFolderFS.Sdk.Accounts.DataModels;

namespace SecureFolderFS.Sdk.Dropbox.DataModels
{
    [Serializable]
    public sealed record DropboxAccountDataModel(string? AccountId, string? DataSourceType, string? DisplayName)
        : AccountDataModel(AccountId, DataSourceType, DisplayName)
    {
        /// <summary>
        /// The short-lived access token. May be expired — the refresh token is used to renew it.
        /// </summary>
        [JsonPropertyName("accessToken")]
        public required string? AccessToken { get; init; }

        /// <summary>
        /// The long-lived refresh token used to silently obtain new access tokens.
        /// </summary>
        [JsonPropertyName("refreshToken")]
        public required string? RefreshToken { get; init; }

        /// <summary>
        /// UTC expiry of the access token. Used to decide whether to refresh proactively.
        /// </summary>
        [JsonPropertyName("expiresAt")]
        public required DateTime? ExpiresAt { get; init; }
    }
}