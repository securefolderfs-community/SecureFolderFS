using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Represents a service for securely storing sensitive information.
    /// </summary>
    public interface IPropertyStoreService
    {
        /// <summary>
        /// Gets a property store for securely managing sensitive data.
        /// </summary>
        /// <remarks>
        /// The <see cref="SecurePropertyStore"/> provides functionality for storing, retrieving, and managing sensitive information
        /// securely by using a key-value mechanism. It ensures the data is accessed and modified securely.
        /// </remarks>
        IPropertyStore<string> SecurePropertyStore { get; }

        // TODO: Use this method for storing settings
        //IDatabaseModel<string> GetDatabaseModelAsync(IFolder persistenceFolder, CancellationToken cancellationToken);
    }
}
