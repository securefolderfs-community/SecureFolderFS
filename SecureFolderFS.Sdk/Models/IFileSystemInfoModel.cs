using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a file system descriptor.
    /// </summary>
    public interface IFileSystemInfoModel
    {
        /// <summary>
        /// Gets the name of this file system.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the unique id associated with this file system.
        /// </summary>
        string FileSystemId { get; }

        /// <summary>
        /// Determines whether this file system is supported by this machine.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> that determines whether the file system is supported.</returns>
        Task<IResult> IsSupportedAsync(CancellationToken cancellationToken = default);
    }
}
