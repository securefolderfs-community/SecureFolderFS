using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a cryptographic cipher descriptor.
    /// </summary>
    public interface ICipherInfoModel : IEquatable<ICipherInfoModel>
    {
        /// <summary>
        /// Gets the name of this cipher.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets an unique id associated with this cipher.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Determines whether this cryptographic cipher is supported by this device.
        /// </summary>
        /// <remarks>
        /// The result may contain additional information that determines whether cipher intrinsics are supported
        /// or device is optimized for this encryption.
        /// </remarks>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> that determines whether the cipher is supported.</returns>
        Task<IResult> IsSupportedAsync(CancellationToken cancellationToken = default);
    }
}
