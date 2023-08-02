using SecureFolderFS.Core;
using SecureFolderFS.Core.Enums;
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
    internal sealed class WebDavFileSystemDescriptor : IFileSystemInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; } = "WebDav";

        /// <inheritdoc/>
        public string Id { get; } = Core.Constants.FileSystemId.WEBDAV_ID;

        /// <inheritdoc/>
        public Task<IResult> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            var result = VaultHelpers.DetermineAvailability(FileSystemAdapterType.WebDavAdapter);
            if (result == FileSystemAvailabilityType.Available)
                return Task.FromResult<IResult>(new FileSystemResult(true, true)); // Always available

            return Task.FromResult<IResult>(new FileSystemResult(true, new NotSupportedException($"WebDav file system is not supported: {result}.")));
        }
    }
}
