namespace SecureFolderFS.Core.FileHeaders
{
    internal abstract class BaseFileHeader : IFileHeader
    {
        public byte[] Nonce { get; }

        public byte[] ContentKey { get; } // TODO: SecretKey here?

        protected BaseFileHeader(byte[] nonce, byte[] contentKey)
        {
            Nonce = nonce;
            ContentKey = contentKey;
        }
    }
}
