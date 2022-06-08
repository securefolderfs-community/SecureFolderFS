namespace SecureFolderFS.Core.Sdk.Streams
{
    public interface ICleartextFileStream : IBaseFileStream
    {
        bool CanBeDeleted();
    }
}
