using DokanNet;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class SetEndOfFileCallback : BaseDokanOperationsCallback, ISetEndOfFileCallback
    {
        public SetEndOfFileCallback(HandlesManager handles)
            : base(handles)
        {
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            try
            {
                if (handles.GetHandle<FileHandle>(GetContextValue(info)) is { } fileHandle)
                {
                    if (length < fileHandle.HandleStream.Length)
                        fileHandle.HandleStream.SetLength(length);

                    return DokanResult.Success;
                }
                else
                {
                    return DokanResult.InvalidHandle;
                }
            }
            catch (IOException)
            {
                return DokanResult.DiskFull;
            }
        }
    }
}
