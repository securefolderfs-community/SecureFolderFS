using SecureFolderFS.Core;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <inheritdoc cref="IFileSystemInfoModel"/>
    internal sealed class DokanyFileSystemDescriptor : IFileSystemInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; } = "Dokany";

        /// <inheritdoc/>
        public string Id { get; } = Core.Constants.FileSystemId.DOKAN_ID;

        /// <inheritdoc/>
        public Task<IResult> IsSupportedAsync(CancellationToken cancellationToken = default)
        {
            var result = VaultHelpers.DetermineAvailability(FileSystemAdapterType.DokanAdapter);
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

                return Task.FromResult<IResult>(new CommonResult(new NotSupportedException(message)));
            }

            return Task.FromResult<IResult>(CommonResult.Success);
        }
    }
}
