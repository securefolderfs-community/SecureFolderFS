using System;
using System.Linq;

namespace SecureFolderFS.Sdk.SecureStore
{
    public abstract class DisposableArray<T> : UnknownStore<DisposableArray<T>>
    {
        protected readonly T[] bytes;

        protected DisposableArray(T[] bytes)
        {
            this.bytes = bytes;
        }

        public override bool Equals(DisposableArray<T>? other)
        {
            return other?.bytes.SequenceEqual(this.bytes) ?? false;
        }

        public override int GetHashCode()
        {
            return bytes.GetHashCode();
        }

        protected override void SecureFree()
        {
            Array.Clear(bytes);
        }
    }
}
