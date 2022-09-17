using DokanNet;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Streams;

namespace SecureFolderFS.Core.Dokany.Callbacks.DokanyCallbacks
{
    internal sealed class UnlockFileCallback : BaseDokanOperationsCallback, IUnlockFileCallback
    {
        public UnlockFileCallback(HandlesManager handles)
            : base(handles)
        {
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            try
            {
                if (handles.GetHandle<FileHandle>(GetContextValue(info)) is { } fileHandle)
                {
                    (fileHandle.HandleStream as ICleartextFileStreamEx)?.Unlock(offset, length);
                    return DokanResult.Success;
                }
                else
                {
                    return DokanResult.InvalidHandle;
                }
            }
            catch (IOException)
            {
                return DokanResult.AccessDenied;
            }
        }
    }
}
