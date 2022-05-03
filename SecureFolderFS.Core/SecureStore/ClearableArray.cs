using SecureFolderFS.Sdk.SecureStore;

namespace SecureFolderFS.Core.SecureStore
{
    internal sealed class ClearableArray<T> : DisposableArray<T>, IExposedBuffer<T>
    {
        T[] IExposedBuffer<T>.Buffer => base.bytes;

        public ClearableArray(T[] bytes)
            : base(bytes)
        {
        }
    }
}
