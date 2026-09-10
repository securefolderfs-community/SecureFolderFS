using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.Api.Models
{
    /// <summary>
    /// The first message a client sends after connecting.
    /// </summary>
    /// <param name="ProtocolMin">The lowest protocol version the client can speak.</param>
    /// <param name="ProtocolMax">The highest protocol version the client can speak.</param>
    /// <param name="ClientName">A human-readable name shown in the consent prompt. Untrusted.</param>
    /// <param name="Token">A token from a previous successful pairing, if the client has one.</param>
    public sealed record HelloRequest(
        [property: JsonPropertyName("protocolMin")] int ProtocolMin,
        [property: JsonPropertyName("protocolMax")] int ProtocolMax,
        [property: JsonPropertyName("clientName")] string? ClientName,
        [property: JsonPropertyName("token")] string? Token = null);

    /// <summary>
    /// The server's answer to <see cref="HelloRequest"/>.
    /// </summary>
    /// <param name="ProtocolVersion">The protocol version that will be used for this session.</param>
    /// <param name="ServerVersion">The SecureFolderFS application version, for diagnostics.</param>
    /// <param name="State">One of <see cref="Constants.SessionStates"/>.</param>
    /// <param name="Capabilities">The optional features this server supports.</param>
    /// <param name="Scopes">The scopes granted to this session, empty when not yet paired.</param>
    public sealed record HelloResponse(
        [property: JsonPropertyName("protocolVersion")] int ProtocolVersion,
        [property: JsonPropertyName("serverVersion")] string ServerVersion,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("capabilities")] IReadOnlyList<string> Capabilities,
        [property: JsonPropertyName("scopes")] IReadOnlyList<string> Scopes);

    /// <summary>
    /// A request to raise the consent prompt and obtain a token.
    /// </summary>
    public sealed record PairRequest
    {
        /// <summary>
        /// Gets the scopes being requested. When omitted, the default scopes are requested.
        /// </summary>
        [JsonPropertyName("scopes")]
        public IReadOnlyList<string>? Scopes { get; init; }
    }

    /// <summary>
    /// A successful pairing.
    /// </summary>
    /// <param name="Token">The bearer token to persist and present in future handshakes.</param>
    /// <param name="Scopes">The scopes the user actually granted.</param>
    public sealed record PairResponse(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("scopes")] IReadOnlyList<string> Scopes);

    /// <summary>
    /// A vault as seen by an integration.
    /// </summary>
    /// <param name="Id">The public, stable identifier.</param>
    /// <param name="Name">The vault's display name.</param>
    /// <param name="State">One of <see cref="Constants.VaultStates"/>.</param>
    /// <param name="MountPath">The plaintext root, present only while unlocked.</param>
    /// <param name="LastAccess">When the vault was last opened, if known.</param>
    public sealed record VaultInfo(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("mountPath")] string? MountPath = null,
        [property: JsonPropertyName("lastAccess")] DateTimeOffset? LastAccess = null);

    /// <summary>
    /// A snapshot of every vault visible to integrations.
    /// </summary>
    /// <param name="Revision">A monotonic revision. A client that observes a gap missed an event and should re-request a snapshot.</param>
    /// <param name="Vaults">The vaults, in the order the user arranged them.</param>
    public sealed record VaultListResponse(
        [property: JsonPropertyName("revision")] long Revision,
        [property: JsonPropertyName("vaults")] IReadOnlyList<VaultInfo> Vaults);

    /// <summary>
    /// A request that targets a single vault.
    /// </summary>
    /// <param name="VaultId">The public identifier from <see cref="VaultInfo.Id"/>.</param>
    public sealed record VaultRequest(
        [property: JsonPropertyName("vaultId")] string? VaultId);

    /// <summary>
    /// A notification carrying a vault's current state.
    /// </summary>
    public sealed record VaultChangedNotification(
        [property: JsonPropertyName("revision")] long Revision,
        [property: JsonPropertyName("vault")] VaultInfo Vault);

    /// <summary>
    /// A notification that a vault is no longer visible to integrations.
    /// </summary>
    public sealed record VaultRemovedNotification(
        [property: JsonPropertyName("revision")] long Revision,
        [property: JsonPropertyName("vaultId")] string VaultId);

    /// <summary>
    /// The outcome of a trigger method.
    /// </summary>
    /// <param name="Status">One of <see cref="Constants.ActionStatus"/>.</param>
    public sealed record ActionResponse(
        [property: JsonPropertyName("status")] string Status);
}
