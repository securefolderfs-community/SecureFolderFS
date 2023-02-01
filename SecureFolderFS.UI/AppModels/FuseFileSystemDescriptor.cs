using SecureFolderFS.Core;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IFileSystemInfoModel"/>
    internal sealed class FuseFileSystemDescriptor : IFileSystemInfoModel
    {
        public string Name { get; } = "FUSE";

        public string Id { get; } = Core.Constants.FileSystemId.FUSE_ID;

        public Task<IResult> IsSupportedAsync(CancellationToken cancellationToken = default)
        {
            var result = VaultHelpers.DetermineAvailability(FileSystemAdapterType.FuseAdapter);
            if (result != FileSystemAvailabilityType.Available)
            {
                // TODO: Use translation strings
                var message = result switch
                {
                    FileSystemAvailabilityType.ModuleNotAvailable or
                    FileSystemAvailabilityType.CoreNotAvailable => "libfuse3 has not been detected. Please install libfuse3 to continue using SecureFolderFS.",
                    _ => "SecureFolderFS cannot work with the installed libfuse version. Please install libfuse3."
                };

                return Task.FromResult<IResult>(new CommonResult(new NotSupportedException(message)));
            }

            return Task.FromResult<IResult>(CommonResult.Success);
        }
    }
}