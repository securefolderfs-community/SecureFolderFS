using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
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

        /// <summary>
        /// Gets a new <see cref="IDatabaseModel{TKey}"/> for storing data in a <paramref name="databaseFile"/>.
        /// </summary>
        /// <param name="databaseFile">The file where data will be stored and read from.</param>
        /// <returns>An instance of <see cref="IDatabaseModel{TKey}"/> which holds data backed by <paramref name="databaseFile"/>.</returns>
        IDatabaseModel<string> GetDatabaseModel(IFile databaseFile);
    }
}
