namespace SecureFolderFS.Core.Sdk.Streams
{
    public interface ICleartextFileStream : IBaseFileStream
    {
        ICiphertextFileStream UnderlyingStream { get; }
    }
}
