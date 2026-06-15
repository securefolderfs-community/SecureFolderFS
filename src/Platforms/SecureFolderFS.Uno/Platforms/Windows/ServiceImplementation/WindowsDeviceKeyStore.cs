#if APP_PLATFORM_PRESENT && WINDOWS
using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Uno.ServiceImplementation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <summary>
    /// Windows <see cref="FileDeviceKeyStore"/> that protects device key material at rest using the DPAPI, scoped to the current user.
    /// </summary>
    internal sealed class WindowsDeviceKeyStore : FileDeviceKeyStore
    {
        private const string PROTECTION_DESCRIPTOR = "LOCAL=user";

        public WindowsDeviceKeyStore(IModifiableFolder baseFolder)
            : base(baseFolder)
        {
        }

        /// <inheritdoc/>
        protected override async Task<byte[]> ProtectAsync(byte[] data, CancellationToken cancellationToken)
        {
            var provider = new DataProtectionProvider(PROTECTION_DESCRIPTOR);
            var protectedBuffer = await provider.ProtectAsync(CryptographicBuffer.CreateFromByteArray(data)).AsTask(cancellationToken);
            CryptographicBuffer.CopyToByteArray(protectedBuffer, out var result);
            
            return result;
        }

        /// <inheritdoc/>
        protected override async Task<byte[]> UnprotectAsync(byte[] data, CancellationToken cancellationToken)
        {
            var provider = new DataProtectionProvider(PROTECTION_DESCRIPTOR);
            var unprotectedBuffer = await provider.UnprotectAsync(CryptographicBuffer.CreateFromByteArray(data)).AsTask(cancellationToken);
            CryptographicBuffer.CopyToByteArray(unprotectedBuffer, out var result);
            
            return result;
        }
    }
}
#endif
