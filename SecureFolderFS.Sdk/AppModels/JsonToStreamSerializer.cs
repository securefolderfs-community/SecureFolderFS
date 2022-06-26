using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public virtual Task<Stream> SerializeAsync<TData>(TData data, CancellationToken cancellationToken = default)
        {
            var serialized = JsonConvert.SerializeObject(data, typeof(TData), Formatting.Indented, null);
            var buffer = Encoding.UTF8.GetBytes(serialized);

            return Task.FromResult<Stream>(new MemoryStream(buffer));
        }

        /// <inheritdoc/>
        public virtual async Task<TData?> DeserializeAsync<TData>(Stream serialized, CancellationToken cancellationToken = default)
        {
            var buffer = await serialized.ReadAllBytesAsync().ConfigureAwait(false);
            var rawSerialized = Encoding.UTF8.GetString(buffer);

            return JsonConvert.DeserializeObject<TData?>(rawSerialized);
        }

        /// <inheritdoc/>
        public virtual TData? EnsureDeserialized<TData>(object? unknown)
        {
            return unknown switch
            {
                null => default,
                TData data => data,
                JToken jtoken => jtoken.ToObject<TData?>(),
                _ => default
            };
        }
    }
}
