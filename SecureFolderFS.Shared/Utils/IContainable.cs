namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Provides method to determine whether an instance contains specific value.
    /// </summary>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    public interface IContainable<in T>
    {
        /// <summary>
        /// Checks if the current instance contains <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The other instance to compare.</param>
        /// <returns>The result is true if the instance contains <paramref name="other"/>, otherwise false.</returns>
        bool Contains(T? other);
    }
}
