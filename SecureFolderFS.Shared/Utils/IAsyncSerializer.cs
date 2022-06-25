using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Provides data serialization abstractions for <typeparamref name="TSerialized"/> data.
    /// </summary>
    /// <typeparam name="TSerialized">The type of data serialized to.</typeparam>
    public interface IAsyncSerializer<TSerialized>
    {
        /// <summary>
        /// Serializes <paramref name="data"/> into <typeparamref name="TSerialized"/>.
        /// </summary>
        /// <typeparam name="TData">The type of data to serialize.</typeparam>
        /// <param name="data">The data to serialize.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <typeparamref name="TSerialized"/> of transformed <paramref name="data"/>.</returns>
        Task<TSerialized> SerializeAsync<TData>(TData data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserializes <paramref name="serialized"/> into <typeparamref name="TData"/>.
        /// </summary>
        /// <typeparam name="TData">The type to deserialize into.</typeparam>
        /// <param name="serialized">The data to deserialize.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <typeparamref name="TData"/> of transformed <paramref name="serialized"/>.</returns>
        Task<TData?> DeserializeAsync<TData>(TSerialized serialized, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ensures that the provided <paramref name="unknown"/> is in deserialized <typeparamref name="TData"/> form.
        /// </summary>
        /// <typeparam name="TData">The deserialized type.</typeparam>
        /// <param name="unknown">The object that may or may not be deserialized.</param>
        /// <returns>Returns <typeparamref name="TData"/> of the <paramref name="unknown"/> data object.</returns>
        TData? EnsureDeserialized<TData>(object? unknown);
    }
}
