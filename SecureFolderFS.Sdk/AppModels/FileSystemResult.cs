using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IResult"/>
    public sealed class FileSystemResult : CommonResult
    {
        /// <summary>
        /// Determines whether the file system is supported by the device and platform.
        /// </summary>
        public bool IsSupported { get; }

        public FileSystemResult(bool isSuccess, bool isSupported)
            : base(isSuccess)
        {
            IsSupported = isSupported;
        }
    }
}
