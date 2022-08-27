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
        public WriteFileCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesManager handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        public NtStatus WriteFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            var ciphertextPath = GetCiphertextPath(fileName);
            var appendToFile = offset == -1;
            var contextHandle = Constants.FileSystem.INVALID_HANDLE;
            var openedNewHandle = false;

            // Memory-mapped
            if (IsContextInvalid(info) || handles.GetHandle<FileHandle>(GetContextValue(info)) is not { } fileHandle)
            {
                // Invalid handle...
                var fileMode = appendToFile ? FileMode.Append : FileMode.Open;
                contextHandle = handles.OpenHandleToFile(ciphertextPath, fileMode, System.IO.FileAccess.ReadWrite, FileShare.None, FileOptions.None);
                fileHandle = handles.GetHandle<FileHandle>(contextHandle)!;
                openedNewHandle = true;
            }

            try
            {
                // Align for Paging I/O
                var alignedBytesToCopy = AlignSizeForPagingIo((int)bufferLength, offset, fileHandle.HandleStream.Length, info);

                // Align position for offset
                var alignedPosition = appendToFile ? fileHandle.HandleStream.Length : offset;

                // Write
                if (openedNewHandle)
                {
                    fileHandle.HandleStream.Position = alignedPosition;
                    bytesWritten = StreamHelpers.WriteFromIntPtrBuffer(fileHandle.HandleStream, buffer, alignedBytesToCopy);
                }
                else
                {
                    lock (fileHandle.HandleStream) // Protect from overlapped write
                    {
                        fileHandle.HandleStream.Position = alignedPosition;
                        bytesWritten = StreamHelpers.WriteFromIntPtrBuffer(fileHandle.HandleStream, buffer, alignedBytesToCopy);
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
            catch (Exception ex)
            {
                _ = ex;
                bytesWritten = 0;

                Debugger.Break();
                return DokanResult.InternalError;
            }
            finally
            {
                if (openedNewHandle)
                    handles.CloseHandle(contextHandle);
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
