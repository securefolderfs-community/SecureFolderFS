namespace SecureFolderFS.Core.SecureStore
{
    internal interface IExposedBuffer<T>
    {
        internal T[] Buffer { get; }
    }
}
