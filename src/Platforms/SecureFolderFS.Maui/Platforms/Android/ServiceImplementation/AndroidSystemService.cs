using OwlCore.Storage;
using SecureFolderFS.Maui.Platforms.Android.Storage;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class AndroidSystemService : ISystemService
    {
        // TODO: Use BroadcastReceiver - ActionScreenOff, ActionUserPresent

        /// <inheritdoc/>
        public event EventHandler? DeviceLocked;

        /// <inheritdoc/>
        public Task<long> GetAvailableFreeSpaceAsync(IFolder storageRoot, CancellationToken cancellationToken = default)
        {
#if ANDROID
            if (storageRoot is not AndroidFolder androidFolder)
                return Task.FromException<long>(new ArgumentNullException(nameof(storageRoot)));

            // fallback: if we can't resolve, assume /storage/emulated/0
            var realPath = TryGetRealPath(androidFolder) ?? "/storage/emulated/0";
            var stat = new global::Android.OS.StatFs(realPath);

            long availableBytes;
#if ANDROID33_0_OR_GREATER
            availableBytes = stat.AvailableBytes;
#else
            availableBytes = (long)stat.BlockSizeLong * (long)stat.AvailableBlocksLong;
#endif

            return Task.FromResult(availableBytes);
            
            static string? TryGetRealPath(AndroidFolder androidFolder)
            {
                if (androidFolder.Inner is not { Scheme: "content", Authority: "com.android.externalstorage.documents" })
                    return null; // Couldn't resolve

                var split = androidFolder.Inner.Path?.Split(':');
                var relativePath = string.Join('/', split?.Skip(1).Take(Range.All) ?? []);
                var type = split?.FirstOrDefault();
                if (type?.Contains("primary", StringComparison.OrdinalIgnoreCase) ?? true)
                    return $"/storage/emulated/0/{relativePath}";

                return $"/storage/{type}/{relativePath}";
            }
#else
    throw new PlatformNotSupportedException("Only implemented on Android.");
#endif
        }

    }
}
