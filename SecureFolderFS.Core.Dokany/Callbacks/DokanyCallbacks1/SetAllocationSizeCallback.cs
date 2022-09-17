using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.Dokany.Callbacks.DokanyCallbacks
{
    internal sealed class SetAllocationSizeCallback : BaseDokanOperationsCallback, ISetAllocationSizeCallback
    {
        private readonly ISetEndOfFileCallback _setEndOfFileCallback;

        public SetAllocationSizeCallback(ISetEndOfFileCallback setEndOfFileCallback, HandlesManager handles)
            : base(handles)
        {
            _setEndOfFileCallback = setEndOfFileCallback;
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            return _setEndOfFileCallback.SetEndOfFile(fileName, length, info);
        }
    }
}
