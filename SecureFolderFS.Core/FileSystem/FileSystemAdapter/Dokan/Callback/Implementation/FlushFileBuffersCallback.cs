using DokanNet;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class FlushFileBuffersCallback : BaseDokanOperationsCallback, IFlushFileBuffersCallback
    {
        public FlushFileBuffersCallback(HandlesManager handles)
            : base(handles)
        {
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            if (handles.GetHandle<FileHandle>(GetContextValue(info)) is not { } fileHandle)
                return DokanResult.InvalidHandle;

            try
            {
                fileHandle.HandleStream.Flush();
                return DokanResult.Success;
            }
            catch (IOException)
            {
                return DokanResult.DiskFull;
            }
        }
    }
}
