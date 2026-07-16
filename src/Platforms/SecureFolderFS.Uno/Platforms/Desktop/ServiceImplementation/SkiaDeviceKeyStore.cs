#if APP_PLATFORM_PRESENT && !WINDOWS
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="FileDeviceKeyStore"/>
    internal sealed class SkiaDeviceKeyStore : FileDeviceKeyStore
    {
        public SkiaDeviceKeyStore(IModifiableFolder baseFolder)
            : base(baseFolder)
        {
        }

        /// <inheritdoc/>
        protected override Task<byte[]> ProtectAsync(byte[] data, CancellationToken cancellationToken)
            => Task.FromResult(data);

        /// <inheritdoc/>
        protected override Task<byte[]> UnprotectAsync(byte[] data, CancellationToken cancellationToken)
            => Task.FromResult(data);
    }
}
#endif
