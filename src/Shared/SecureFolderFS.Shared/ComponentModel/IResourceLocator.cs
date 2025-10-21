namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents an interface that provides functionality for locating resources by a unique key.
    /// </summary>
    /// <typeparam name="T">The type of the resource that this locator retrieves.</typeparam>
    public interface IResourceLocator<out T>
    {
        /// <summary>
        /// Tries to get the resource for the provided <paramref name="resourceKey"/>/
        /// </summary>
        /// <param name="resourceKey">The resource key that uniquely associates a given resource.</param>
        /// <returns>If successful, returns the associated resource; otherwise null.</returns>
        T? GetResource(string resourceKey);
    }
}
