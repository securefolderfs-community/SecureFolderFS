using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Extensions
{
    public static class SerializationExtensions
    {
        public static async Task<TSerialized> SerializeAsync<TSerialized, TData>(
            this IAsyncSerializer<TSerialized> serializer,
            TData? data,
            CancellationToken cancellationToken = default)
        {
            return await serializer.SerializeAsync(data, typeof(TData), cancellationToken);
        }

        public static async Task<TData?> DeserializeAsync<TSerialized, TData>(
            this IAsyncSerializer<TSerialized> serializer,
            TSerialized serialized,
            CancellationToken cancellationToken = default)
        {
            return (TData?)await serializer.DeserializeAsync(serialized, typeof(TData), cancellationToken);
        }

        public static async Task<TSerialized?> TrySerializeAsync<TSerialized, TData>(
            this IAsyncSerializer<TSerialized> serializer,
            TData? data,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await SerializeAsync(serializer, data, cancellationToken);
            }
            catch (Exception ex)
            {
                _ = ex;
                return default;
            }
        }

        public static async Task<TData?> TryDeserializeAsync<TSerialized, TData>(
            this IAsyncSerializer<TSerialized> serializer,
            TSerialized serialized,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await DeserializeAsync<TSerialized, TData>(serializer, serialized, cancellationToken);
            }
            catch (Exception ex)
            {
                _ = ex;
                return default;
            }
        }
    }
}
