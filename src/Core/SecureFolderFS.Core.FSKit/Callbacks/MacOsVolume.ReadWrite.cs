#pragma warning disable APL0002
using System;
using Foundation;
using FSKit;

namespace SecureFolderFS.Core.FSKit.Callbacks
{
    internal sealed partial class MacOsVolume : IFSVolumeReadWriteOperations
    {
        /// <inheritdoc/>
        public unsafe void Read(FSItem item, long offset, UIntPtr length, FSMutableFileDataBuffer buffer,
            FSVolumeReadWriteOperationsReadHandler reply)
        {
            var bufferSpan = new Span<byte>(buffer.MutableBytes.ToPointer(), (int)length.ToUInt32());
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Write(NSData contents, FSItem item, long offset, FSVolumeReadWriteOperationsWriteHandler reply)
        {
            using var stream = contents.AsStream();
            throw new NotImplementedException();
        }
    }
}