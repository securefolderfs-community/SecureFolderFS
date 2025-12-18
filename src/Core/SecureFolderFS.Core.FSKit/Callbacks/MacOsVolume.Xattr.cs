#pragma warning disable APL0002
using System;
using Foundation;
using FSKit;

namespace SecureFolderFS.Core.FSKit.Callbacks
{
    internal sealed partial class MacOsVolume : IFSVolumeXattrOperations
    {
        /// <inheritdoc/>
        public void GetXattr(FSFileName name, FSItem item, FSVolumeXattrOperationsGetHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetXattr(FSFileName name, NSData? value, FSItem item, FSSetXattrPolicy policy,
            FSVolumeXattrOperationsSetHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ListXattrs(FSItem item, FSVolumeXattrOperationsListHandler reply)
        {
            throw new NotImplementedException();
        }
    }
}