using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <inheritdoc cref="IFileSystemInfoModel"/>
    internal sealed class DokanyFileSystemDescriptor : IFileSystemInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; } = "Dokany";

        /// <inheritdoc/>
        public string FileSystemId { get; } = Core.Constants.FileSystemId.DOKAN_ID;

        /// <inheritdoc/>
        public Task<IResult> IsSupportedAsync(CancellationToken cancellationToken = default)
        {
            var result = FileSystemAvailabilityHelpers.IsDokanyAvailable();
            if (result != FileSystemAvailabilityErrorType.FileSystemAvailable)
            {
                var message = result switch
                {
                    FileSystemAvailabilityErrorType.ModuleNotAvailable or
                    FileSystemAvailabilityErrorType.DriverNotAvailable => "Dokany has not been detected. Please install Dokany to continue using SecureFolderFS.",
                    FileSystemAvailabilityErrorType.VersionTooLow => "The installed version of Dokany is outdated. Please update Dokany to the match requested version.",
                    FileSystemAvailabilityErrorType.VersionTooHigh => "The installed version of Dokany is not compatible with SecureFolderFS version. Please install requested version of Dokany.",
                    _ => "SecureFolderFS cannot work with installed Dokany version. Please install requested version of Dokany."
                };

                return Task.FromResult<IResult>(new CommonResult(new UnavailableFileSystemAdapterException(message)));
            }

            return Task.FromResult<IResult>(new CommonResult());
        }
    }
}
