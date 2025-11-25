using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.Ipc
{
    /// <summary>
    /// Represents a request sent via Inter-Process Communication (IPC).
    /// </summary>
    public sealed class IpcRequest : IpcMessage
    {
        /// <summary>
        /// The command to execute.
        /// </summary>
        [JsonPropertyName("command")]
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Additional parameters for the command.
        /// </summary>
        [JsonPropertyName("parameters")]
        public Dictionary<string, object>? Parameters { get; set; }
    }
}