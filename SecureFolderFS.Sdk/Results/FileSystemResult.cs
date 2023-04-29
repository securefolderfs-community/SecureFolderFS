using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;

namespace SecureFolderFS.Sdk.Results
{
    /// <inheritdoc cref="IResult"/>
    public sealed class FileSystemResult : CommonResult
    {
        /// <summary>
        /// Gets the value that determines whether the device can support a given file system.
        /// <remarks>
        /// The value might be true even when the file system is unavailable.</remarks>
        /// </summary>
        public bool CanSupport { get; }

        public FileSystemResult(bool canSupport, bool isSuccess)
            : base(isSuccess)
        {
            CanSupport = canSupport;
        }

        public FileSystemResult(bool canSupport, Exception? exception)
            : base(exception)
        {
            CanSupport = canSupport;
        }
    }
}
