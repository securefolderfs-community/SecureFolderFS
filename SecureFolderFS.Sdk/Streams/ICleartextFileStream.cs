namespace SecureFolderFS.Sdk.Streams
{
    public interface ICleartextFileStream : IBaseFileStream
    {
        bool CanBeDeleted();
    }
}
