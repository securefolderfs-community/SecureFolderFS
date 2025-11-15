using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Extensions
{
    public static class SerializationExtensions
    {
        public static async Task<string?> TrySerializeToStringAsync<TData>(
            this IAsyncSerializer<Stream> serializer,
            TData? data,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await SerializeToStringAsync(serializer, data, cancellationToken);
            }
            catch (Exception ex)
            {
                _ = ex;
                return null;
            }
        }

        public static async Task<TResult?> TryDeserializeFromStringAsync<TResult>(
            this IAsyncSerializer<Stream> serializer,
            string serialized,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await DeserializeFromStringAsync<TResult>(serializer, serialized, cancellationToken);
            }
            catch (Exception ex)
            {
                _ = ex;
                return default;
            }
        }

        public static async Task<string> SerializeToStringAsync<TData>(
            this IAsyncSerializer<Stream> serializer,
            TData? data,
            CancellationToken cancellationToken = default)
        {
            await using var stream = await serializer.SerializeAsync(data, cancellationToken);
            using var streamReader = new StreamReader(stream);

            return await streamReader.ReadToEndAsync(cancellationToken);
        }

        public static async Task<TResult?> DeserializeFromStringAsync<TResult>(
            this IAsyncSerializer<Stream> serializer,
            string serialized,
            CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream(serialized.Length);
            await using var streamWriter = new StreamWriter(stream);

            await streamWriter.WriteAsync(serialized);
            await streamWriter.FlushAsync(cancellationToken);

            return await serializer.DeserializeAsync<Stream, TResult?>(stream, cancellationToken);
        }

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
