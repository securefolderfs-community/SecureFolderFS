using DokanNet;
using System;
using System.Collections.Generic;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class FindStreamsCallback : BaseDokanOperationsCallbackWithPath, IFindStreamsCallback // TODO: For now, BaseDokanOperationsCallbackWithPath is not used
    {
        public FindStreamsCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            streams = Array.Empty<FileInformation>();
            return DokanResult.NotImplemented;
        }
    }
}
