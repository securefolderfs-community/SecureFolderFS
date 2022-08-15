using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Implementation for <see cref="IAsyncSerializer{TSerialized}"/> that uses <see cref="Stream"/> to serialize/deserialize JSON.
    /// </summary>
    public class JsonToStreamSerializer : IAsyncSerializer<Stream>
    {
        /// <summary>
        /// A single instance of <see cref="JsonToStreamSerializer"/>.
        /// </summary>
        public static JsonToStreamSerializer Instance { get; } = new();

        protected JsonToStreamSerializer()
        {
        }

        /// <inheritdoc/>
        public virtual Task<Stream> SerializeAsync(object? data, Type dataType, CancellationToken cancellationToken = default)
        {
            var serialized = JsonConvert.SerializeObject(data, dataType, Formatting.Indented, null);
            var buffer = Encoding.UTF8.GetBytes(serialized);

            return Task.FromResult<Stream>(new MemoryStream(buffer));
        }

        /// <inheritdoc/>
        public virtual async Task<object?> DeserializeAsync(Stream serialized, Type dataType, CancellationToken cancellationToken = default)
        {
            var buffer = await serialized.ReadAllBytesAsync();
            var rawSerialized = Encoding.UTF8.GetString(buffer);

            return JsonConvert.DeserializeObject(rawSerialized, dataType);
        }
    }
}
