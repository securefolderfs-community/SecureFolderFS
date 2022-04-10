using System;
using System.Linq;
using System.Runtime.InteropServices;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.UnsafeNative;

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
            EnsureSecureDisposal(Bytes);
        }

        internal static void EnsureSecureDisposal(byte[] buffer)
        {
            var bufHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr bufPtr = bufHandle.AddrOfPinnedObject();
                UIntPtr cnt = new UIntPtr((uint)buffer.Length * (uint)sizeof(byte));

                UnsafeNativeApis.RtlZeroMemory(bufPtr, cnt);
            }
            finally
            {
                bufHandle.Free();
            }
        }

        public static implicit operator byte[](DisposableArray disposableArray) => disposableArray.Bytes;
    }
}
