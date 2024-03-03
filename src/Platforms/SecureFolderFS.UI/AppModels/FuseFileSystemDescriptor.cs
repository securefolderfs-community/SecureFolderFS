using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FUSE;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IFileSystemInfoModel"/>
    internal sealed class FuseFileSystemDescriptor : IFileSystemInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; } = "FUSE";

        /// <inheritdoc/>
        public string Id { get; } = Core.Constants.FileSystemId.FS_FUSE;

        /// <inheritdoc/>
        public Task<IResult> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            var result = FuseMountable.IsSupported();
            if (result != FileSystemAvailabilityType.Available)
            {
                // TODO: Use translation strings
                var message = result switch
                {
                    FileSystemAvailabilityType.ModuleNotAvailable or
                    FileSystemAvailabilityType.CoreNotAvailable => "libfuse3 has not been detected. Please install libfuse3 to continue using SecureFolderFS.",
                    _ => "SecureFolderFS cannot work with the installed libfuse version. Please install libfuse3."
                };

                return Task.FromResult<IResult>(new FileSystemResult(OperatingSystem.IsLinux(), new NotSupportedException(message)));
            }

            return Task.FromResult<IResult>(new FileSystemResult(OperatingSystem.IsLinux(), true));
        }
    }
}
