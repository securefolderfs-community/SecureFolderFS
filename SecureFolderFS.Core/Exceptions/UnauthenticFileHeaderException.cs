namespace SecureFolderFS.Core.Exceptions
{
    public sealed class UnauthenticFileHeaderException : IntegrityException
    {
        public UnauthenticFileHeaderException()
            : base("File header has been tampered. Authentication tag does not match the calculated tag.")
        {
        }
    }
}