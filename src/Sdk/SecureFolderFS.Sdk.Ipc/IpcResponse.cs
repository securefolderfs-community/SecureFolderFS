using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.Ipc
{
    /// <summary>
    /// IPC response message sent from FSKit service to Bridge.
    /// </summary>
    public sealed class IpcResponse : IpcMessage
    {
        /// <summary>
        /// The status of the response (success, error, etc.).
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable message describing the response.
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Additional data returned by the command.
        /// </summary>
        [JsonPropertyName("data")]
        public Dictionary<string, object>? Data { get; set; }

        /// <summary>
        /// Error details if Status is "error".
        /// </summary>
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}