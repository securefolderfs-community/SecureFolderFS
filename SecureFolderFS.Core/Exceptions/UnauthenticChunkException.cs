namespace SecureFolderFS.Core.Exceptions
{
    public sealed class UnauthenticChunkException : IntegrityException
    {
        public UnauthenticChunkException()
            : base("Chunk has been tampered. Authentication tag does not match the calculated tag.")
        {
        }
    }
}
