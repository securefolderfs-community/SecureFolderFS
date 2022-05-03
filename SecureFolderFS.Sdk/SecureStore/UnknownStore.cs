using System;

namespace SecureFolderFS.Sdk.SecureStore
{
    public abstract class UnknownStore<TImplementation> : IDisposable, IEquatable<TImplementation>
        where TImplementation : class
    {
        protected bool Disposed { get; private set; }

        ~UnknownStore()
        {
            NotSecureFree();
        }

        public sealed override bool Equals(object? obj)
        {
            return Equals(obj as TImplementation);
        }

        protected virtual void NotSecureFree()
        {
        }

        public abstract bool Equals(TImplementation? other);

        public abstract override int GetHashCode();

        protected abstract void SecureFree();

        public void Dispose()
        {
            SecureFree();
            Disposed = true;
        }
    }
}
