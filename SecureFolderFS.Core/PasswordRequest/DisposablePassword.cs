using SecureFolderFS.Core.SecureStore;
using System;

namespace SecureFolderFS.Core.PasswordRequest
{
    /// <summary>
    /// Provides implementation for password with on-demand <see cref="IDisposable"/> disposing.
    /// <br/>
    /// <br/>
    /// This SDK is exposed.
    /// </summary>
    public sealed class DisposablePassword : IDisposable
    {
        internal SecretKey Password { get; }

        public int Length => Password.Key.Length;

        public DisposablePassword(byte[] password)
        {
            Password = new(password);
        }

        public static DisposablePassword AsEmpty()
        {
            return new DisposablePassword(Array.Empty<byte>());
        }

        public void Dispose()
        {
            Password.Dispose();
        }
    }
}
