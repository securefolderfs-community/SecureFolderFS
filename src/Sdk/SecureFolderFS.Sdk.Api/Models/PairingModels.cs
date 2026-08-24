using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.Api.Models
{
    /// <summary>
    /// An application the user has authorized to use the local API.
    /// </summary>
    /// <param name="Id">A stable internal identifier for this pairing.</param>
    /// <param name="DisplayName">The name shown in the paired-clients list.</param>
    /// <param name="TokenHash">The Base64 SHA-256 digest of the issued token.</param>
    /// <param name="WasIdentityVerified">Whether the platform verified the publisher at pairing time.</param>
    /// <param name="ExecutablePath">Where the client executable lived at pairing time.</param>
    /// <param name="Signer">The verified publisher, when one was established.</param>
    /// <param name="Scopes">The permissions the user granted.</param>
    /// <param name="CreatedAt">When the pairing was established.</param>
    /// <param name="LastUsedAt">When the pairing was last used to authenticate.</param>
    public sealed record PairedClient(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("displayName")] string DisplayName,
        [property: JsonPropertyName("tokenHash")] string TokenHash,
        [property: JsonPropertyName("wasIdentityVerified")] bool WasIdentityVerified,
        [property: JsonPropertyName("executablePath")] string? ExecutablePath,
        [property: JsonPropertyName("signer")] string? Signer,
        [property: JsonPropertyName("scopes")] IReadOnlyList<string> Scopes,
        [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt,
        [property: JsonPropertyName("lastUsedAt")] DateTimeOffset? LastUsedAt);

    /// <summary>
    /// Records that the user refused an application, so it cannot immediately ask again.
    /// </summary>
    /// <param name="Fingerprint">A digest identifying the refused caller.</param>
    /// <param name="DeniedAt">When the refusal happened.</param>
    public sealed record DeniedClient(
        [property: JsonPropertyName("fingerprint")] string Fingerprint,
        [property: JsonPropertyName("deniedAt")] DateTimeOffset DeniedAt);

    public sealed class PairingStoreDataModel
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; set; } = 1;

        [JsonPropertyName("vaultIdKey")]
        public string VaultIdKey { get; set; } = string.Empty;

        [JsonPropertyName("clients")]
        public List<PairedClient> Clients { get; set; } = [];

        [JsonPropertyName("denied")]
        public List<DeniedClient> Denied { get; set; } = [];

        public PairingStoreDataModel Clone() => new()
        {
            SchemaVersion = SchemaVersion,
            VaultIdKey = VaultIdKey,
            Clients = [.. Clients],
            Denied = [.. Denied]
        };
    }
}
