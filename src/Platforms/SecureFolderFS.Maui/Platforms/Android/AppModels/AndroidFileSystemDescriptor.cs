using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.MobileFS.Platforms.Android;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Maui.Platforms.Android.AppModels
{
    /// <inheritdoc cref="IFileSystemInfoModel"/>
    internal sealed class AndroidFileSystemDescriptor : IFileSystemInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; } = "Android SAF";

        /// <inheritdoc/>
        public string Id { get; } = Core.Constants.FileSystemId.FS_ANDROID;

        /// <inheritdoc/>
        public async Task<IResult> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            var result = AndroidFileSystemMountable.IsSupported();
            if (result == FileSystemAvailabilityType.Available)
                return new FileSystemResult(OperatingSystem.IsAndroid(), true);

            var message = result switch
            {
                // TODO: Handle more cases
                _ => "DocumentsProvider is not available.",
            };

            await Task.CompletedTask;
            return new FileSystemResult(OperatingSystem.IsAndroid(), new NotSupportedException(message));
        }
    }
}
