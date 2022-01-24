using DokanNet;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class FlushFileBuffersCallback : BaseDokanOperationsCallback, IFlushFileBuffersCallback
    {
        public FlushFileBuffersCallback(HandlesCollection handles)
            : base(handles)
        {
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            if (IsContextInvalid(info))
            {
                return DokanResult.InvalidHandle;
            }
            else if (info.IsDirectory)
            {
                return DokanResult.AccessDenied;
            }
            else if (handles.GetHandle(GetContextValue(info)) is FileHandle fileHandle)
            {
                try
                {
                    fileHandle.CleartextFileStream.Flush();
                    return DokanResult.Success;
                }
                catch (IOException)
                {
                    return DokanResult.DiskFull;
                }
            }
            else
            {
                return DokanResult.InvalidHandle;
            }
        }
    }
}
