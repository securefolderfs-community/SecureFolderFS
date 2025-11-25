using System;
using FSKit;

namespace SecureFolderFS.Core.FSKit.Callbacks
{
    internal sealed partial class MacOsVolume : IFSVolumeOpenCloseOperations
    {
        /// <inheritdoc/>
        public void OpenItem(FSItem item, FSVolumeOpenModes mode, FSVolumeOpenCloseOperationsHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void CloseItem(FSItem item, FSVolumeOpenModes mode, FSVolumeOpenCloseOperationsHandler reply)
        {
            throw new NotImplementedException();
        }
    }
}