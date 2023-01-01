using SecureFolderFS.Core.FileSystem;
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
    internal sealed class WebDavFileSystemDescriptor : IFileSystemInfoModel
    {
        private IFileSystemAvailabilityChecker _availabilityChecker;

        /// <inheritdoc/>
        public string Name { get; } = "WebDav";

        /// <inheritdoc/>
        public string Id { get; } = Core.Constants.FileSystemId.WEBDAV_ID;

        public WebDavFileSystemDescriptor(IFileSystemAvailabilityChecker availabilityChecker)
        {
            _availabilityChecker = availabilityChecker;
        }

        /// <inheritdoc/>
        public async Task<IResult> IsSupportedAsync(CancellationToken cancellationToken = default)
        {
            var availabilityType = await _availabilityChecker.DetermineAvailabilityAsync();
            if (availabilityType == FileSystemAvailabilityType.Available)
                return CommonResult.Success;

            return new CommonResult(new NotSupportedException($"WebDav file system is not supported: {availabilityType.ToString()}."));
        }

        /// <inheritdoc/>
        public bool Equals(IFileSystemInfoModel? other)
        {
            if (other is null)
                return false;

            return other.Id.Equals(Id);
        }
    }
}
