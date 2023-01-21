using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

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
        /// Gets an unique id associated with this file system.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Determines whether this file system is supported by the device.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> that determines whether the file system is supported.</returns>
        Task<IResult> IsSupportedAsync(CancellationToken cancellationToken = default);
    }
}
