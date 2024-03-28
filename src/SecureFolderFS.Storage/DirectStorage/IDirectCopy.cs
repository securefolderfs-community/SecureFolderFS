using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.DirectStorage
{
    /// <summary>
    /// Provides direct copy operation of storage objects.
    /// </summary>
    public interface IDirectCopy : IModifiableFolder
    {
        /// <summary>
        /// Creates a copy of the provided storable item in this folder.
        /// </summary>
        Task<IStorableChild> CreateCopyOfAsync(IStorableChild itemToCopy, bool overwrite = default, CancellationToken cancellationToken = default);
    }
}
