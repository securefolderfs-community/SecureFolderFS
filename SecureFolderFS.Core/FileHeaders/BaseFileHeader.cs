using System;

namespace SecureFolderFS.Core.FileHeaders
{
    internal abstract class BaseFileHeader : IFileHeader
    {
        public byte[] Nonce { get; }

        public byte[] ContentKey { get; } // TODO: SecretKey here?

        protected BaseFileHeader(byte[] nonce, byte[] contentKey)
        {
            this.Nonce = nonce;
            this.ContentKey = contentKey;
        }

        public virtual void Dispose()
        {
            Array.Clear(Nonce);
            Array.Clear(ContentKey);
        }
    }
}
