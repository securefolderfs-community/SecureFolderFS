using DokanNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class ReadFileCallback : BaseDokanOperationsCallbackWithPath, IReadFileCallback
    {
        public ReadFileCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesManager handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset, IDokanFileInfo info)
        {
            var ciphertextPath = GetCiphertextPath(fileName);
            var contextHandle = Constants.FileSystem.INVALID_HANDLE;
            var openedNewHandle = false;

            // Memory-mapped
            if (IsContextInvalid(info) || handles.GetHandle<FileHandle>(GetContextValue(info)) is not { } fileHandle)
            {
                // Invalid handle...
                contextHandle = handles.OpenHandleToFile(ciphertextPath, FileMode.Open, System.IO.FileAccess.Read, FileShare.Read, FileOptions.None);
                fileHandle = handles.GetHandle<FileHandle>(contextHandle)!;
                openedNewHandle = true;
            }

            try
            {
                // Check EOF
                if (offset >= fileHandle.HandleStream.Length)
                {
                    bytesRead = 0;
                    return Constants.IO.FILE_EOF;
                }
                else
                {
                    fileHandle.HandleStream.Position = offset;
                }

                // Read file
                bytesRead = StreamHelpers.ReadToIntPtrBuffer(fileHandle.HandleStream, buffer, (int)bufferLength);
                
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
            catch (Exception ex)
            {
                _ = ex;
                bytesRead = 0;

                Debugger.Break();
                return DokanResult.InternalError;
            }
            finally
            {
                if (openedNewHandle)
                {
                    handles.CloseHandle(contextHandle);
                }
            }
        }
    }
}
