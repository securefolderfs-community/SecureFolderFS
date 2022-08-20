using DokanNet;
using System;
using System.Diagnostics;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class ReadFileCallback : BaseDokanOperationsCallbackWithPath, IReadFileCallback
    {
        public ReadFileCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        public NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset, IDokanFileInfo info)
        {
            ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);

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
                // Check EOF
                if (offset >= fileHandle.CleartextFileStream.Length)
                {
                    bytesRead = 0;
                    return Constants.IO.FILE_EOF;
                }
                else
                {
                    fileHandle.CleartextFileStream.Position = offset;
                }

                // Read file
                if (openedNewHandle)
                {
                    bytesRead = StreamHelpers.ReadToIntPtrBuffer(fileHandle.CleartextFileStream, buffer, (int)bufferLength);
                }
                else
                {
                    lock (fileHandle!.CleartextFileStream)
                    {
                        bytesRead = StreamHelpers.ReadToIntPtrBuffer(fileHandle.CleartextFileStream, buffer, (int)bufferLength);
                    }
                }
                
                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                bytesRead = 0;
                return DokanResult.InvalidName;
            }
            catch (IntegrityException)
            {
                bytesRead = 0;
                return NtStatus.CrcError;
            }
            catch (UnavailableStreamException)
            {
                bytesRead = 0;
                return NtStatus.HandleNoLongerValid;
            }
            catch (Exception)
            {
                bytesRead = 0;

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
    }
}
