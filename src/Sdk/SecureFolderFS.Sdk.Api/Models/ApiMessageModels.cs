using System.Text.Json;
using System.Text.Json.Serialization;
using SecureFolderFS.Sdk.Api.Serialization;

namespace SecureFolderFS.Sdk.Api.Protocol
{
    /// <summary>
    /// A single API message that describes a request, response, or notification.
    /// </summary>
    public sealed record ApiMessage
    {
        /// <summary>
        /// Gets the correlation identifier, or <see langword="null"/> for a notification.
        /// </summary>
        [JsonPropertyName("id")]
        public long? Id { get; init; }

        /// <summary>
        /// Gets the invoked method or the emitted event name.
        /// </summary>
        [JsonPropertyName("method")]
        public string? Method { get; init; }

        /// <summary>
        /// Gets the request or notification payload.
        /// </summary>
        [JsonPropertyName("params")]
        public object? Params { get; init; }

        /// <summary>
        /// Gets the successful response payload.
        /// </summary>
        [JsonPropertyName("result")]
        public object? Result { get; init; }

        /// <summary>
        /// Gets the failure details, when the request did not succeed.
        /// </summary>
        [JsonPropertyName("error")]
        public ApiError? Error { get; init; }

        /// <summary>
        /// Creates a request message.
        /// </summary>
        public static ApiMessage Request(long id, string method, object? parameters = null)
            => new() { Id = id, Method = method, Params = parameters };

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        public static ApiMessage Response(long id, object? result)
            => new() { Id = id, Result = result };

        /// <summary>
        /// Creates a failure response.
        /// </summary>
        public static ApiMessage Failure(long? id, string code, string message, int? retryAfterMs = null)
            => new() { Id = id, Error = new ApiError(code, message, retryAfterMs) };

        /// <summary>
        /// Creates a server-to-client notification, which carries no identifier.
        /// </summary>
        public static ApiMessage Notification(string method, object? parameters)
            => new() { Method = method, Params = parameters };

        /// <summary>
        /// Deserializes <see cref="Params"/> into <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <returns>The deserialized object, or <see langword="null"/> when absent or not decodable.</returns>
        public T? GetParams<T>() where T : class
        {
            if (Params is not JsonElement element)
                return Params as T;

            if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                return null;

            return element.Deserialize<T>(ApiSerializer.Options);
        }
    }

    /// <summary>
    /// A structured failure returned in place of a result.
    /// </summary>
    /// <param name="Code">A stable machine-readable code from <see cref="Constants.ErrorCodes"/>.</param>
    /// <param name="Message">A human-readable description.</param>
    /// <param name="RetryAfterMs">When rate limited, how long to wait before retrying.</param>
    public sealed record ApiError(
        [property: JsonPropertyName("code")] string Code,
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("retryAfterMs")] int? RetryAfterMs = null);
}
