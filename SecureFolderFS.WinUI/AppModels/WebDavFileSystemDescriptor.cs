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
    internal sealed class WebDavFileSystemDescriptor : IFileSystemInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; } = "WebDav";

        /// <inheritdoc/>
        public string Id { get; } = Core.Constants.FileSystemId.WEBDAV_ID;

        /// <inheritdoc/>
        public Task<IResult> IsSupportedAsync(CancellationToken cancellationToken = default)
        {
            var result = VaultHelpers.DetermineAvailability(FileSystemAdapterType.WebDavAdapter);
            if (result == FileSystemAvailabilityType.Available)
                return Task.FromResult<IResult>(CommonResult.Success);

            return Task.FromResult<IResult>(new CommonResult(new NotSupportedException($"WebDav file system is not supported: {result}.")));
        }
    }
}
