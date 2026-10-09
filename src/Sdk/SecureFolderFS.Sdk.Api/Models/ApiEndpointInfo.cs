using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.Api.Models
{
    /// <summary>
    /// Describes where the API can be reached, and whether it is available at all.
    /// </summary>
    public sealed record ApiEndpointInfo(
        [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
        [property: JsonPropertyName("transport")] string Transport,
        [property: JsonPropertyName("address")] string Address,
        [property: JsonPropertyName("enabled")] bool Enabled,
        [property: JsonPropertyName("protocolMin")] int ProtocolMin,
        [property: JsonPropertyName("protocolMax")] int ProtocolMax,
        [property: JsonPropertyName("capabilities")] IReadOnlyList<string> Capabilities,
        [property: JsonPropertyName("appVersion")] string? AppVersion = null,
        [property: JsonPropertyName("executablePath")] string? ExecutablePath = null,
        [property: JsonPropertyName("processId")] int? ProcessId = null);
}