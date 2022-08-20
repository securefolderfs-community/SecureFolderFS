using DokanNet;
using System;
using System.IO;
using System.Diagnostics;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Paths;

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
            ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);

            var appendToFile = offset == -1;
            var contextHandle = Constants.FileSystem.INVALID_HANDLE;
            var openedNewHandle = false;

            // Memory-mapped
            if (IsContextInvalid(info) || handles.GetHandle(GetContextValue(info)) is not FileHandle fileHandle)
            {
                // Invalid handle...
                contextHandle = handles.OpenHandleToFile(ciphertextPath, FileMode.Open, System.IO.FileAccess.Read, FileShare.Read, FileOptions.None);
                fileHandle = (FileHandle)handles.GetHandle(contextHandle);
                openedNewHandle = true;
            }

            try
            {
                // Align for Paging I/O
                var alignedBytesToCopy = AlignSizeForPagingIo((int)bufferLength, offset, fileHandle.CleartextFileStream.Length, info);

                // Align position for offset
                var alignedPosition = appendToFile ? fileHandle.CleartextFileStream.Length : offset;

                // Write
                if (openedNewHandle)
                {
                    fileHandle.CleartextFileStream.Position = alignedPosition;
                    bytesWritten = StreamHelpers.WriteFromIntPtrBuffer(fileHandle.CleartextFileStream, buffer, alignedBytesToCopy);
                }
                else
                {
                    lock (fileHandle.CleartextFileStream) // Protect from overlapped write
                    {
                        fileHandle.CleartextFileStream.Position = alignedPosition;
                        bytesWritten = StreamHelpers.WriteFromIntPtrBuffer(fileHandle.CleartextFileStream, buffer, alignedBytesToCopy);
                    }
                }

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
            catch (UnavailableStreamException)
            {
                bytesWritten = 0;
                return DokanResult.InvalidHandle;
            }
            catch (Exception)
            {
                bytesWritten = 0;

                Debugger.Break();
                return DokanResult.InternalError;
            }
            finally
            {
                if (openedNewHandle)
                {
                    handles.Close(contextHandle);
                }
            }
        }

        private static int AlignSizeForPagingIo(int bufferLength, long offset, long streamLength, IDokanFileInfo info)
        {
            if (info.PagingIo)
            {
                var longDistanceToEnd = streamLength - offset;
                if (longDistanceToEnd > int.MaxValue)
                    return bufferLength;

                if (longDistanceToEnd < bufferLength)
                    return (int)longDistanceToEnd;

                return bufferLength;
            }

            return bufferLength;
        }
    }
}
