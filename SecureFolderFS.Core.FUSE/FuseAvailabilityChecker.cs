using System.Runtime.InteropServices;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;

namespace SecureFolderFS.Core.FUSE
{
    /// <inheritdoc cref="IFileSystemAvailabilityChecker"/>
    public sealed class FuseAvailabilityChecker : IFileSystemAvailabilityChecker
    {
        /// <inheritdoc/>
        public Task<FileSystemAvailabilityType> DetermineAvailabilityAsync()
        {
            var handle = NativeLibrary.Load("libfuse3.so.3");
            if (handle == IntPtr.Zero)
                return Task.FromResult(FileSystemAvailabilityType.ModuleNotAvailable);

            NativeLibrary.Free(handle);
            return Task.FromResult(FileSystemAvailabilityType.Available);
        }
    }
}