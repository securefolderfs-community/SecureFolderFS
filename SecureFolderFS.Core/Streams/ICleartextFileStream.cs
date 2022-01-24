namespace SecureFolderFS.Core.Streams
{
    public interface ICleartextFileStream : IBaseFileStream
    {
        bool CanBeDeleted();
    }
}
