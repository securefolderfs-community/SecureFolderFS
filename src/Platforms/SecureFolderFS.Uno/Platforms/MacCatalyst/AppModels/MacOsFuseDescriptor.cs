using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FUSE;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.AppModels
{
    /// <inheritdoc cref="IFileSystemInfoModel"/>
    internal sealed class MacOsFuseDescriptor : IFileSystemInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; } = Core.FUSE.Constants.FILE_SYSTEM_NAME;

        /// <inheritdoc/>
        public string Id { get; } = Core.Constants.FileSystemId.FS_FUSE;

        /// <inheritdoc/>
        public Task<IResult> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            var result = FuseMountable.IsSupported();
            if (result != FileSystemAvailabilityType.Available)
            {
                // TODO: Use localization strings
                var message = result switch
                {
                    FileSystemAvailabilityType.ModuleUnavailable or
                    FileSystemAvailabilityType.CoreUnavailable => "libfuse3 has not been detected. Please install libfuse3 to continue using SecureFolderFS.",
                    _ => "SecureFolderFS cannot work with the installed libfuse version. Please install libfuse3."
                };

                return Task.FromResult<IResult>(new FileSystemResult(OperatingSystem.IsMacCatalyst(), new NotSupportedException(message)));
            }

            return Task.FromResult<IResult>(new FileSystemResult(OperatingSystem.IsMacCatalyst(), true));
        }
    }
}
