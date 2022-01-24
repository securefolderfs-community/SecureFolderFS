namespace SecureFolderFS.Core.Exceptions
{
    public sealed class UnauthenticChunkException : IntegrityException
    {
        private UnauthenticChunkException(string message)
            : base(message)
        {
        }

        public static UnauthenticChunkException ForAesCtrHmac()
        {
            return new UnauthenticChunkException("Chunk has been tampered. MAC does not match actual MAC.");
        }

        public static UnauthenticChunkException ForAesGcm()
        {
            return new UnauthenticChunkException("Chunk has been tampered. Tag doesn't match actual tag.");
        }

        public static UnauthenticChunkException ForXChaCha20()
        {
            return new UnauthenticChunkException("Chunk has been tampered. Tag doesn't match actual tag.");
        }
    }
}
