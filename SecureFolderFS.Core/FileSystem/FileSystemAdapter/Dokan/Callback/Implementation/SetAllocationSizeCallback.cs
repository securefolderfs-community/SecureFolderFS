using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class SetAllocationSizeCallback : BaseDokanOperationsCallback, ISetAllocationSizeCallback
    {
        private readonly ISetEndOfFileCallback _setEndOfFileCallback;

        public SetAllocationSizeCallback(ISetEndOfFileCallback setEndOfFileCallback, HandlesCollection handles)
            : base(handles)
        {
            this._setEndOfFileCallback = setEndOfFileCallback;
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            return _setEndOfFileCallback.SetEndOfFile(fileName, length, info);
        }
    }
}
