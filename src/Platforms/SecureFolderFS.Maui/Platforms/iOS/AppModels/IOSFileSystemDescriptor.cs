using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.MobileFS;
using SecureFolderFS.Core.MobileFS.Platforms.iOS;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="IFileSystemInfoModel"/>
    internal sealed class IOSFileSystemDescriptor : IFileSystemInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; } = Constants.ANDROID_FILE_SYSTEM_NAME;

        /// <inheritdoc/>
        public string Id { get; } = Core.Constants.FileSystemId.FS_ANDROID;

        /// <inheritdoc/>
        public async Task<IResult> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            var result = IOSFileSystemMountable.IsSupported();
            if (result == FileSystemAvailabilityType.Available)
                return new FileSystemResult(OperatingSystem.IsAndroid(), true);

            await Task.CompletedTask;
            return new FileSystemResult(OperatingSystem.IsAndroid(), new NotSupportedException("DocumentsProvider is not available."));
        }
    }
}
