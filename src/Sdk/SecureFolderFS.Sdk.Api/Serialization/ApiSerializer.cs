using System.Text.Json;
using System.Text.Json.Serialization;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.Api.Serialization
{
    /// <summary>
    /// The single JSON configuration used for every API message and persisted file. Writing is
    /// non-indented so a payload never breaks the one-message-per-line wire framing, and null members are omitted.
    /// </summary>
    public sealed class ApiSerializer : StreamSerializer
    {
        /// <summary>
        /// Gets the shared API serializer instance.
        /// </summary>
        public new static ApiSerializer Instance { get; } = new();

        /// <summary>
        /// Gets the underlying options, for the synchronous file paths that cannot await a stream.
        /// </summary>
        public static JsonSerializerOptions Options => Instance.SerializerOptions;

        private ApiSerializer() : base(new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = false,
            NumberHandling = JsonNumberHandling.Strict
        })
        {
        }
    }
}
