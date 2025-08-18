using SecureFolderFS.Shared.ComponentModel;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// Implementation for <see cref="IAsyncSerializer{TSerialized}"/> that uses <see cref="Stream"/> to serialize/deserialize JSON.
    /// </summary>
    public class StreamSerializer : IAsyncSerializer<Stream>
    {
        protected JsonSerializerOptions SerializerOptions { get; }

        /// <summary>
        /// A single instance of <see cref="StreamSerializer"/>.
        /// </summary>
        public static StreamSerializer Instance { get; } = new();

        protected StreamSerializer(JsonSerializerOptions? options = null)
        {
            SerializerOptions = options ?? new()
            {
                WriteIndented = true
            };
        }

        /// <inheritdoc/>
        public virtual async Task<Stream> SerializeAsync(object? data, Type dataType, CancellationToken cancellationToken = default)
        {
            var outputStream = new MemoryStream();

            // Serialize data to stream
            await JsonSerializer.SerializeAsync(outputStream, data, dataType, SerializerOptions, cancellationToken);
            outputStream.Position = 0;

            return outputStream;
        }

        /// <inheritdoc/>
        public virtual async Task<object?> DeserializeAsync(Stream serialized, Type dataType, CancellationToken cancellationToken = default)
        {
            if (serialized.CanSeek)
                serialized.Position = 0L;

            return await JsonSerializer.DeserializeAsync(serialized, dataType, SerializerOptions, cancellationToken);
        }
    }
}
