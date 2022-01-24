using DokanNet;
using System;
using System.IO;
using System.Diagnostics;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Exceptions;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class WriteFileCallback : BaseDokanOperationsCallbackWithPath, IWriteFileCallback
    {
        public WriteFileCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        public NtStatus WriteFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            if (info.IsDirectory)
            {
                bytesWritten = 0;
                return DokanResult.AccessDenied;
            }

            bool appendToFile = info.WriteToEndOfFile || offset == -1;
            if (appendToFile && info.PagingIo)
            {
                // Paging IO to end, do nothing
                bytesWritten = 0;
                return DokanResult.Success;
            }
            else if (info.PagingIo)
            {
                // TODO: Adjust offset
            }

            ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);
            long contextHandle = GetContextValue(info);
            bool opened = false;

            try
            {
                if (handles.GetHandle(contextHandle) is not FileHandle fileHandle)
                {
                    // Invalid handle...
                    contextHandle = handles.OpenHandleToFile(ciphertextPath, FileMode.Open, System.IO.FileAccess.ReadWrite, FileShare.Read, FileOptions.None);
                    fileHandle = (FileHandle)handles.GetHandle(contextHandle);

                    opened = true;
                }

                // Write file
                bytesWritten = StreamHelpers.WriteFromIntPtrBuffer(fileHandle.CleartextFileStream, buffer, bufferLength, appendToFile ? fileHandle.CleartextFileStream.Length : offset);
                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                bytesWritten = 0;
                return DokanResult.InvalidName;
            }
            catch (IntegrityException)
            {
                bytesWritten = 0;
                return NtStatus.CrcError;
            }
            catch (Exception ex)
            {
                _ = ex;
                bytesWritten = 0;

                Debugger.Break();
                return DokanResult.InternalError;
            }
            finally
            {
                if (opened)
                {
                    handles.Close(contextHandle);
                }
            }
        }
    }
}
