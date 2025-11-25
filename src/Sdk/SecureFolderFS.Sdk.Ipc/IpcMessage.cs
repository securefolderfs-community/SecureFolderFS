using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.Ipc
{
    /// <summary>
    /// Represents the base class for messages exchanged via Inter-Process Communication (IPC).
    /// </summary>
    public abstract class IpcMessage
    {
        /// <summary>
        /// Unique identifier for correlating requests and responses.
        /// </summary>
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Timestamp of when the message was created.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
