namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Allows an object represented by <typeparamref name="T"/> to be wrapped into <see cref="IWrapper{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to wrap.</typeparam>
    public interface IWrappable<T>
    {
        /// <summary>
        /// Wraps an object into <see cref="IWrapper{T}"/>.
        /// </summary>
        /// <param name="obj">The object to wrap.</param>
        /// <returns>A wrapped object by the implementation represented by <see cref="IWrapper{T}"/>.</returns>
        IWrapper<T> Wrap(T obj);
    }
}
