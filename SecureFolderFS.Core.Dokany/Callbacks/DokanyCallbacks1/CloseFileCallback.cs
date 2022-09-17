using DokanNet;
using SecureFolderFS.Core.Dokany.OpenHandles;
using SecureFolderFS.Core.FileSystem.Paths;

namespace SecureFolderFS.Core.Dokany.Callbacks.DokanyCallbacks
{
    internal sealed class CloseFileCallback : BaseDokanCallback, ICloseFileCallback
    {
        public CloseFileCallback(string vaultRootPath, IPathConverter pathConverter, HandlesManager handlesManager)
            : base(vaultRootPath, pathConverter, handlesManager)
        {
        }

        public void CloseFile(string fileName, IDokanFileInfo info)
        {
            CloseHandle(info);
            InvalidateContext(info);
        }
    }
}
