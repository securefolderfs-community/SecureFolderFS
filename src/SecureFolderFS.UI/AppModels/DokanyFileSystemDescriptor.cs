using SecureFolderFS.Core.Dokany;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IFileSystemInfoModel"/>
    internal sealed class DokanyFileSystemDescriptor : IFileSystemInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; } = "Dokany";

        /// <inheritdoc/>
        public string Id { get; } = Core.Constants.FileSystemId.DOKAN_ID;

        /// <inheritdoc/>
        public Task<IResult> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            var result = DokanyMountable.IsSupported();
            if (result != FileSystemAvailabilityType.Available)
            {
                // TODO: Use translation strings
                var message = result switch
                {
                    FileSystemAvailabilityType.ModuleNotAvailable or
                    FileSystemAvailabilityType.CoreNotAvailable => "Dokany has not been detected. Please install Dokany to continue using SecureFolderFS.",
                    FileSystemAvailabilityType.VersionTooLow => "The installed version of Dokany is outdated. Please update Dokany to the match requested version.",
                    FileSystemAvailabilityType.VersionTooHigh => "The installed version of Dokany is not compatible with SecureFolderFS version. Please install requested version of Dokany.",
                    _ => "SecureFolderFS cannot work with installed Dokany version. Please install requested version of Dokany."
                };

                return Task.FromResult<IResult>(new FileSystemResult(OperatingSystem.IsWindows(), new NotSupportedException(message)));
            }

            return Task.FromResult<IResult>(new FileSystemResult(OperatingSystem.IsWindows(), true));
        }
    }
}
