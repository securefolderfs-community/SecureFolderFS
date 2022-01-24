namespace SecureFolderFS.Core.Exceptions
{
    public sealed class UnauthenticFileHeaderException : IntegrityException
    {
        private UnauthenticFileHeaderException(string message)
            : base(message)
        {
        }

        public static UnauthenticFileHeaderException ForAesCtrHmac()
        {
            return new UnauthenticFileHeaderException("File header has been tampered. MAC does not match actual MAC.");
        }

        public static UnauthenticFileHeaderException ForAesGcm()
        {
            return new UnauthenticFileHeaderException("File header has been tampered. Tag doesn't match actual tag.");
        }

        public static UnauthenticFileHeaderException ForXChaCha20()
        {
            return new UnauthenticFileHeaderException("File header has been tampered. Tag doesn't match actual tag.");
        }
    }
}
