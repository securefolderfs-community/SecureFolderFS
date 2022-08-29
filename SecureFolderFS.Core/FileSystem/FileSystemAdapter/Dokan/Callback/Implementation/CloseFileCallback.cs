using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class CloseFileCallback : BaseDokanOperationsCallback, ICloseFileCallback
    {
        public CloseFileCallback(HandlesManager handles)
            : base(handles)
        {
        }

        public void CloseFile(string fileName, IDokanFileInfo info)
        {
            CloseHandle(info);
            InvalidateContext(info);
        }
    }
}
