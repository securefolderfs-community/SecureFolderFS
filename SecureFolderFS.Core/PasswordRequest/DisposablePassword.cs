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
        internal byte[] Password { get; }

        public DisposablePassword(byte[] password)
        {
            this.Password = password;
        }

        public void Dispose()
        {
            Array.Clear(Password);
        }
    }
}
