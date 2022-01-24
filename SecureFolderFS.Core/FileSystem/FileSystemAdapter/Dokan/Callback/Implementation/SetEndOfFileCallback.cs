using DokanNet;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class SetEndOfFileCallback : BaseDokanOperationsCallback, ISetEndOfFileCallback
    {
        public SetEndOfFileCallback(HandlesCollection handles)
            : base(handles)
        {
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            if (IsContextInvalid(info))
            {
                return DokanResult.InvalidHandle;
            }
            else if (info.IsDirectory)
            {
                return DokanResult.AccessDenied;
            }

            try
            {
                if (handles.GetHandle(GetContextValue(info)) is FileHandle fileHandle)
                {
                    if (length < fileHandle.CleartextFileStream.Length)
                    {
                        fileHandle.CleartextFileStream.SetLength(length);
                    }

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
