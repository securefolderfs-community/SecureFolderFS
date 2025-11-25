using System;
using Foundation;
using FSKit;

namespace SecureFolderFS.Core.FSKit.Callbacks
{
    internal sealed partial class MacOsVolume : IFSVolumeReadWriteOperations
    {
        /// <inheritdoc/>
        public void Read(FSItem item, long offset, UIntPtr length, FSMutableFileDataBuffer buffer,
            FSVolumeReadWriteOperationsReadHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Write(NSData contents, FSItem item, long offset, FSVolumeReadWriteOperationsWriteHandler reply)
        {
            throw new NotImplementedException();
        }
    }
}