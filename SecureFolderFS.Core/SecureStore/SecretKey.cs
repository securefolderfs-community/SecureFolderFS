using System;
using SecureFolderFS.Core.Sdk.SecureStore;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.SecureStore
{
    internal sealed class SecretKey : IDisposable
    {
        private readonly LockedArray<byte> _internal;

        private readonly IExposedBuffer<byte> _exposedBuffer;

        public byte[] Key => _exposedBuffer.Buffer;

        public SecretKey(byte[] key)
        {
            _internal = EnsureCorrectImplementation(key);
            _exposedBuffer = _internal as IExposedBuffer<byte>;
        }

        public SecretKey CreateCopy()
        {
            return new(Key.CloneArray());
        }

        private static LockedArray<byte> EnsureCorrectImplementation(byte[] key)
        {
            if (CompatibilityHelpers.IsPlatformWindows)
            {
                return new Win32LockedArray<byte>(key);
            }

            throw new PlatformNotSupportedException();
        }

        public void Dispose()
        {
            _internal.Dispose();
        }

        public static implicit operator byte[](SecretKey secretKey) => secretKey.Key;
    }
}
