using System;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.Chunks.Implementation
{
    internal abstract class BaseCiphertextChunk : ICiphertextChunk
    {
        public byte[] Nonce { get; }

        public byte[] Payload { get; }

        public byte[] Auth { get; }

        protected BaseCiphertextChunk(byte[] nonce, byte[] payload, byte[] auth)
        {
            this.Nonce = nonce;
            this.Payload = payload;
            this.Auth = auth;
        }

        public virtual byte[] ToArray()
        {
            var fullChunk = new byte[Nonce.Length + Payload.Length + Auth.Length];
            fullChunk.EmplaceArrays(Nonce, Payload, Auth);

            return fullChunk;
        }

        public virtual void Dispose()
        {
            Array.Clear(Nonce);
            Array.Clear(Payload);
            Array.Clear(Auth);
        }
    }
}
