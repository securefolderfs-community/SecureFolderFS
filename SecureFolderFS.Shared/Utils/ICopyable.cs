namespace SecureFolderFS.Shared.Utils
{
    public interface ICopyable<T> where T : class
    {
        T CreateCopy();
    }
}
