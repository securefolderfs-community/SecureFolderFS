using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Shared.ComponentModel;
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
            var result = WebDavMountable.IsSupported();
            if (result == FileSystemAvailabilityType.Available)
                return Task.FromResult<IResult>(new FileSystemResult(true, true)); // Always available

            return Task.FromResult<IResult>(new FileSystemResult(true, new NotSupportedException($"WebDav file system is not supported: {result}.")));
        }
    }
}
