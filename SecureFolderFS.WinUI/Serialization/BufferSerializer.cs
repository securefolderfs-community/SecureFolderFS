using Newtonsoft.Json;
using SecureFolderFS.Shared.Utils;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.Serialization
{
    /// <summary>
    /// Implementation for <see cref="IAsyncSerializer{TSerialized}"/> that uses <see cref="byte"/> array to serialize/deserialize JSON.
    /// </summary>
    internal sealed class BufferSerializer : IAsyncSerializer<byte[]>
    {
        /// <summary>
        /// A single instance of <see cref="StreamSerializer"/>.
        /// </summary>
        public static BufferSerializer Instance { get; } = new();

        private BufferSerializer()
        {
        }

        /// <inheritdoc/>
        public Task<byte[]> SerializeAsync(object? data, Type dataType, CancellationToken cancellationToken = default)
        {
            var serialized = JsonConvert.SerializeObject(data, dataType, Formatting.Indented, null);
            return Task.FromResult(Encoding.UTF8.GetBytes(serialized));
        }

        /// <inheritdoc/>
        public Task<object?> DeserializeAsync(byte[] serialized, Type dataType, CancellationToken cancellationToken = default)
        {
            var rawSerialized = Encoding.UTF8.GetString(serialized);
            return Task.FromResult(JsonConvert.DeserializeObject(rawSerialized, dataType));
        }
    }
}
