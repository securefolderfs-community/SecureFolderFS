using DokanNet;
using SecureFolderFS.Core.Dokany.Helpers;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Exceptions;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using FileAccess = DokanNet.FileAccess;

namespace SecureFolderFS.Core.Dokany.Callbacks
{
    internal abstract class BaseDokanyCallbacks : IDokanOperationsUnsafe, IDisposable
    {
        protected readonly BaseHandlesManager handlesManager;
        protected readonly VolumeModel volumeModel;

        public FileSystemSpecifics Specifics { get; }

        protected BaseDokanyCallbacks(FileSystemSpecifics specifics, BaseHandlesManager handlesManager, VolumeModel volumeModel)
        {
            Specifics = specifics;
            this.handlesManager = handlesManager;
            this.volumeModel = volumeModel;
        }

        #region Unused

        /// <inheritdoc/>
        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            bytesWritten = 0;
            return DokanResult.NotImplemented;
        }

        /// <inheritdoc/>
        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            bytesRead = 0;
            return DokanResult.NotImplemented;
        }

        #endregion

        /// <inheritdoc/>
        public virtual void CloseFile(string fileName, IDokanFileInfo info)
        {
            _ = fileName;

            CloseHandle(info);
            InvalidateContext(info);
        }

        /// <inheritdoc/>
        public virtual NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            if (handlesManager.GetHandle<FileHandle>(GetContextValue(info)) is not { } fileHandle)
                return Trace(DokanResult.InvalidHandle, fileName, info);

            try
            {
                fileHandle.Stream.Flush();
                return Trace(DokanResult.Success, fileName, info);
            }
            catch (IOException)
            {
                return DokanResult.DiskFull;
            }
        }

        /// <inheritdoc/>
        public virtual NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
            return FindFilesWithPattern(fileName, "*", out files, info);
        }

        /// <inheritdoc/>
        public virtual NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            if (Specifics.Options.IsReadOnly)
                return Trace(DokanResult.AccessDenied, fileName, info);

            if (handlesManager.GetHandle<FileHandle>(GetContextValue(info)) is not { } fileHandle)
                return Trace(DokanResult.InvalidHandle, fileName, info);

            fileHandle.Stream.SetLength(length);
            return Trace(DokanResult.Success, fileName, info);
        }

        /// <inheritdoc/>
        public virtual NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            return SetEndOfFile(fileName, length, info);
        }

        /// <inheritdoc/>
        public virtual NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName,
            out uint maximumComponentLength, IDokanFileInfo info)
        {
            volumeLabel = volumeModel.VolumeName;
            fileSystemName = volumeModel.FileSystemName;
            maximumComponentLength = Constants.Dokan.MAX_COMPONENT_LENGTH;
            features = Constants.Dokan.FEATURES;

            return Trace(DokanResult.Success, null, info);
        }

        /// <inheritdoc/>
        public virtual NtStatus Mounted(string mountPoint, IDokanFileInfo info)
        {
            _ = mountPoint; // TODO: Check if mountPoint is different and update the RootFolder (?)
            return Trace(DokanResult.Success, null, info);
        }

        /// <inheritdoc/>
        public virtual NtStatus Unmounted(IDokanFileInfo info)
        {
            return Trace(DokanResult.Success, null, info);
        }

        /// <inheritdoc/>
        public virtual NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            streams = Array.Empty<FileInformation>();
            return Trace(DokanResult.NotImplemented, fileName, info);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public virtual unsafe NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset,
            IDokanFileInfo info)
        {
            var ciphertextPath = GetCiphertextPath(fileName);
            var contextHandle = FileSystem.Constants.INVALID_HANDLE;
            var openedNewHandle = false;

            // Check if the path is correct
            if (ciphertextPath is null)
            {
                bytesRead = 0;
                return Trace(DokanResult.PathNotFound, fileName, info);
            }

            // Memory-mapped
            if (handlesManager.GetHandle<FileHandle>(GetContextValue(info)) is not { } fileHandle)
            {
                // Invalid handle...
                contextHandle = handlesManager.OpenFileHandle(ciphertextPath, FileMode.Open, System.IO.FileAccess.Read, FileShare.Read, FileOptions.None);
                fileHandle = handlesManager.GetHandle<FileHandle>(contextHandle);
                openedNewHandle = true;
            }

            // Re-check handle
            if (fileHandle is null)
            {
                bytesRead = 0;
                return Trace(DokanResult.AccessDenied, fileName, info);
            }

            try
            {
                // Check EOF
                if (offset >= fileHandle.Stream.Length)
                {
                    bytesRead = 0;
                    return NtStatus.EndOfFile;
                }

                // Align position
                fileHandle.Stream.Position = offset;

                // Read file
                var bufferSpan = new Span<byte>(buffer.ToPointer(), (int)bufferLength);
                bytesRead = fileHandle.Stream.Read(bufferSpan);

                return Trace(DokanResult.Success, fileName, info);
            }
            catch (PathTooLongException)
            {
                bytesRead = 0;
                return Trace(DokanResult.InvalidName, fileName, info);
            }
            catch (CryptographicException)
            {
                bytesRead = 0;
                return Trace(NtStatus.CrcError, fileName, info);
            }
            catch (UnavailableStreamException)
            {
                bytesRead = 0;
                return Trace(NtStatus.HandleNoLongerValid, fileName, info);
            }
            finally
            {
                if (openedNewHandle)
                    handlesManager.CloseHandle(contextHandle);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public virtual unsafe NtStatus WriteFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            if (Specifics.Options.IsReadOnly)
            {
                bytesWritten = 0;
                return Trace(DokanResult.AccessDenied, fileName, info);
            }

            var ciphertextPath = GetCiphertextPath(fileName);
            var appendToFile = offset == -1;
            var contextHandle = FileSystem.Constants.INVALID_HANDLE;
            var openedNewHandle = false;

            // Check if the path is correct
            if (ciphertextPath is null)
            {
                bytesWritten = 0;
                return Trace(DokanResult.PathNotFound, fileName, info);
            }

            // Memory-mapped
            if (handlesManager.GetHandle<FileHandle>(GetContextValue(info)) is not { } fileHandle)
            {
                // Invalid handle...
                contextHandle = handlesManager.OpenFileHandle(ciphertextPath, appendToFile ? FileMode.Append : FileMode.Open, System.IO.FileAccess.ReadWrite, FileShare.Read, FileOptions.None);
                fileHandle = handlesManager.GetHandle<FileHandle>(contextHandle);
                openedNewHandle = true;
            }

            // Re-check handle
            if (fileHandle is null)
            {
                bytesWritten = 0;
                return Trace(DokanResult.AccessDenied, fileName, info);
            }

            try
            {
                // Align for Paging I/O
                var alignedBytesToCopy = AlignSizeForPagingIo((int)bufferLength, offset, fileHandle.Stream.Length, info);

                // Align position for offset
                var alignedPosition = appendToFile ? fileHandle.Stream.Length : offset;

                // Align position
                fileHandle.Stream.Position = alignedPosition;

                // Write file
                var bufferSpan = new ReadOnlySpan<byte>(buffer.ToPointer(), alignedBytesToCopy);
                fileHandle.Stream.Write(bufferSpan);
                bytesWritten = alignedBytesToCopy;

                return Trace(DokanResult.Success, fileName, info);
            }
            catch (PathTooLongException)
            {
                bytesWritten = 0;
                return Trace(DokanResult.InvalidName, fileName, info);
            }
            catch (CryptographicException)
            {
                bytesWritten = 0;
                return Trace(NtStatus.CrcError, fileName, info);
            }
            catch (UnavailableStreamException)
            {
                bytesWritten = 0;
                return Trace(NtStatus.HandleNoLongerValid, fileName, info);
            }
            catch (UnauthorizedAccessException)
            {
                bytesWritten = 0;
                return Trace(DokanResult.AccessDenied, fileName, info);
            }
            catch (IOException ioEx)
            {
                if (DokanyErrorHelpers.NtStatusFromException(ioEx, out var ntStatus))
                {
                    bytesWritten = 0;
                    return Trace((NtStatus)ntStatus, fileName, info);
                }

                throw;
            }
            finally
            {
                if (openedNewHandle)
                    handlesManager.CloseHandle(contextHandle);
            }
        }

        /// <inheritdoc/>
        public abstract NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract void Cleanup(string fileName, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus DeleteFile(string fileName, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus DeleteDirectory(string fileName, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus GetFileSecurity(string fileName, out FileSystemSecurity? security, AccessControlSections sections, IDokanFileInfo info);

        /// <inheritdoc/>
        public abstract NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info);

        // TODO: Add checks for nullable in places where this function is called
        protected abstract string? GetCiphertextPath(string plaintextName);

        protected void CloseHandle(IDokanFileInfo info)
        {
            handlesManager.CloseHandle(GetContextValue(info));
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            Specifics.Dispose();
            handlesManager.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool IsContextInvalid(IDokanFileInfo info)
        {
            return info.Context is not ulong ctxUlong || ctxUlong == FileSystem.Constants.INVALID_HANDLE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void InvalidateContext(IDokanFileInfo info)
        {
            info.Context = FileSystem.Constants.INVALID_HANDLE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static ulong GetContextValue(IDokanFileInfo info)
        {
            return info.Context is ulong ctxUlong ? ctxUlong : FileSystem.Constants.INVALID_HANDLE;
        }

        protected static int AlignSizeForPagingIo(int bufferLength, long offset, long streamLength, IDokanFileInfo info)
        {
            if (!info.PagingIo)
                return bufferLength;

            var longDistanceToEnd = streamLength - offset;
            if (longDistanceToEnd > int.MaxValue)
                return bufferLength;

            if (longDistanceToEnd < bufferLength)
                return (int)longDistanceToEnd;

            return bufferLength;
        }

        protected static NtStatus Trace(NtStatus result, string fileName, IDokanFileInfo info,
            FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, [CallerMemberName] string methodName = "")
        {
#if !DEBUG
            return result;
#else
            if (Debugger.IsAttached)
                return result;

            if (!Core.FileSystem.Constants.OPT_IN_FOR_OPTIONAL_DEBUG_TRACING)
                return result;

            if (DisallowedTraceMethods.Contains(methodName))
                return result;

            var message = FormatProviders.DokanFormat($"{methodName}('{fileName}', {info}, [{access}], [{share}], [{mode}], [{options}], [{attributes}]) -> {result}");
            Debug.WriteLine(message);

            return result;
#endif
        }

        protected static NtStatus Trace(NtStatus result, string? fileName, IDokanFileInfo info, [CallerMemberName] string methodName = "", params object[]? args)
        {
#if !DEBUG
            return result;
#endif

            if (!Core.FileSystem.Constants.OPT_IN_FOR_OPTIONAL_DEBUG_TRACING)
                return result;

            if (!Debugger.IsAttached)
                return result;

            if (DisallowedTraceMethods.Contains(methodName))
                return result;

            var extraParameters = args is not null && args.Length > 0
                ? ", " + string.Join(", ", args.Select(x => string.Format(FormatProviders.DefaultFormatProvider, "{0}", x)))
                : string.Empty;

            var message = FormatProviders.DokanFormat($"{methodName}('{fileName}', {info}{extraParameters}) -> {result}");
            Debug.WriteLine(message);

            return result;
        }

        private static string[] DisallowedTraceMethods { get; } =
        {
            "GetVolumeInformation"
        };
    }
}
