using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Dokany;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Uno.Platforms.Windows.AppModels
{
    /// <inheritdoc cref="IFileSystemInfoModel"/>
    internal sealed class WindowsDokanyDescriptor : IFileSystemInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; } = Core.Dokany.Constants.FILE_SYSTEM_NAME;

        /// <inheritdoc/>
        public string Id { get; } = Core.Constants.FileSystemId.FS_DOKAN;

        /// <inheritdoc/>
        public async Task<IResult> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            var result = DokanyMountable.IsSupported();
            if (result == FileSystemAvailabilityType.Available)
                return new FileSystemResult(OperatingSystem.IsWindows(), true);

            // TODO: Use localization strings
            var message = result switch
            {
                FileSystemAvailabilityType.ModuleUnavailable or FileSystemAvailabilityType.CoreUnavailable => "Dokany has not been detected. Please install Dokany to continue using SecureFolderFS.",
                FileSystemAvailabilityType.VersionTooLow => "The installed version of Dokany is outdated. Please update Dokany to the match requested version.",
                FileSystemAvailabilityType.VersionTooHigh => "The installed version of Dokany is not compatible with SecureFolderFS version. Please install requested version of Dokany.",
                _ => "SecureFolderFS cannot work with installed Dokany version. Please install requested version of Dokany."
            };

            await Task.CompletedTask;
            return new FileSystemResult(OperatingSystem.IsWindows(), new NotSupportedException(message));
        }
    }
}
