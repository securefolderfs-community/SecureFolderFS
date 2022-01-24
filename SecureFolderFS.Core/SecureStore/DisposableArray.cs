using System;
using System.Linq;
using SecureFolderFS.Core.Extensions;

namespace SecureFolderFS.Core.SecureStore
{
    internal sealed class DisposableArray : FreeableStore<DisposableArray>
    {
        public byte[] Bytes { get; }

        public DisposableArray(byte[] array)
        {
            this.Bytes = array;
        }

        public override DisposableArray CreateCopy()
        {
            return new DisposableArray(Bytes.CloneArray());
        }

        public override bool Equals(DisposableArray other)
        {
            if (other?.Bytes == null || Bytes == null)
            {
                return false;
            }

            return Bytes.SequenceEqual(other.Bytes);
        }

        public override int GetHashCode()
        {
            return Bytes.GetHashCode();
        }

        protected override void SecureFree()
        {
            Array.Clear(Bytes);
        }

        public static implicit operator byte[](DisposableArray disposableArray) => disposableArray.Bytes;
    }
}
