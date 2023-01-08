using DokanNet;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.Dokany.Helpers;
using SecureFolderFS.Core.Dokany.OpenHandles;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Exceptions;
using SecureFolderFS.Core.FileSystem.Paths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using FileAccess = DokanNet.FileAccess;

namespace SecureFolderFS.Core.Dokany.Callbacks
{
    internal abstract class BaseDokanyCallbacks : IDokanOperationsUnsafe
    {
        protected readonly IPathConverter pathConverter;
        protected readonly HandlesManager handlesManager;
        protected readonly DokanyVolumeModel volumeModel;
        protected readonly IFileSystemHealthStatistics? fileSystemHealthStatistics;

        protected BaseDokanyCallbacks(IPathConverter pathConverter, HandlesManager handlesManager, DokanyVolumeModel volumeModel, IFileSystemHealthStatistics? fileSystemHealthStatistics)
        {
            this.pathConverter = pathConverter;
            this.handlesManager = handlesManager;
            this.volumeModel = volumeModel;
            this.fileSystemHealthStatistics = fileSystemHealthStatistics;
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
                return DokanResult.InvalidHandle;

            try
            {
                fileHandle.Stream.Flush();
                return DokanResult.Success;
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
            if (handlesManager.GetHandle<FileHandle>(GetContextValue(info)) is { } fileHandle)
            {
                fileHandle.Stream.SetLength(length);
                return DokanResult.Success;
            }

            return DokanResult.InvalidHandle;
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
            maximumComponentLength = volumeModel.MaximumComponentLength;
            features = volumeModel.FileSystemFeatures;

            return DokanResult.Success;
        }

        /// <inheritdoc/>
        public virtual NtStatus Mounted(string mountPoint, IDokanFileInfo info)
        {
            _ = mountPoint; // TODO: Check if mountPoint is different and update the RootFolder (?)
            return DokanResult.Success;
        }

        /// <inheritdoc/>
        public virtual NtStatus Unmounted(IDokanFileInfo info)
        {
            return DokanResult.Success;
        }

        /// <inheritdoc/>
        public virtual NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            streams = Array.Empty<FileInformation>();
            return DokanResult.NotImplemented;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public virtual unsafe NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset,
            IDokanFileInfo info)
        {
            var ciphertextPath = GetCiphertextPath(fileName);
            var contextHandle = Constants.FileSystem.INVALID_HANDLE;
            var openedNewHandle = false;

            // Check if the path is correct
            if (ciphertextPath is null)
            {
                bytesRead = 0;
                return DokanResult.PathNotFound;
            }

            // Memory-mapped
            if (IsContextInvalid(info) || handlesManager.GetHandle<FileHandle>(GetContextValue(info)) is not { } fileHandle)
            {
                // Invalid handle...
                contextHandle = handlesManager.OpenHandleToFile(ciphertextPath, FileMode.Open, System.IO.FileAccess.Read, FileShare.Read, FileOptions.None);
                fileHandle = handlesManager.GetHandle<FileHandle>(contextHandle)!;
                openedNewHandle = true;
            }

            try
            {
                // Check EOF
                if (offset >= fileHandle.Stream.Length)
                {
                    bytesRead = 0;
                    return FileSystem.Constants.FILE_EOF;
                }
                else
                    fileHandle.Stream.Position = offset;

                // Read file
                var bufferSpan = new Span<byte>(buffer.ToPointer(), (int)bufferLength);
                bytesRead = fileHandle.Stream.Read(bufferSpan);

                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                bytesRead = 0;
                return DokanResult.InvalidName;
            }
            catch (CryptographicException)
            {
                bytesRead = 0;
                return NtStatus.CrcError;
            }
            catch (UnavailableStreamException)
            {
                bytesRead = 0;
                return NtStatus.HandleNoLongerValid;
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
            var ciphertextPath = GetCiphertextPath(fileName);
            var appendToFile = offset == -1;
            var contextHandle = Constants.FileSystem.INVALID_HANDLE;
            var openedNewHandle = false;

            // Check if the path is correct
            if (ciphertextPath is null)
            {
                bytesWritten = 0;
                return DokanResult.PathNotFound;
            }

            // Memory-mapped
            if (IsContextInvalid(info) || handlesManager.GetHandle<FileHandle>(GetContextValue(info)) is not { } fileHandle)
            {
                // Invalid handle...
                contextHandle = handlesManager.OpenHandleToFile(ciphertextPath, appendToFile ? FileMode.Append : FileMode.Open, System.IO.FileAccess.ReadWrite, FileShare.Read, FileOptions.None);
                fileHandle = handlesManager.GetHandle<FileHandle>(contextHandle)!;
                openedNewHandle = true;
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

                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                bytesWritten = 0;
                return DokanResult.InvalidName;
            }
            catch (CryptographicException)
            {
                bytesWritten = 0;
                return NtStatus.CrcError;
            }
            catch (UnavailableStreamException)
            {
                bytesWritten = 0;
                return NtStatus.HandleNoLongerValid;
            }
            catch (IOException ioEx)
            {
                if (ErrorHandlingHelpers.NtStatusFromException(ioEx, out var ntStatus))
                {
                    bytesWritten = 0;
                    return (NtStatus)ntStatus;
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
        protected abstract string? GetCiphertextPath(string cleartextName);

        protected void CloseHandle(IDokanFileInfo info)
        {
            handlesManager.CloseHandle(GetContextValue(info));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool IsContextInvalid(IDokanFileInfo info)
        {
            return GetContextValue(info) == Constants.FileSystem.INVALID_HANDLE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void InvalidateContext(IDokanFileInfo info)
        {
            info.Context = Constants.FileSystem.INVALID_HANDLE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static long GetContextValue(IDokanFileInfo info)
        {
            return info.Context is long ctxLong ? ctxLong : Constants.FileSystem.INVALID_HANDLE;
        }

        protected static int AlignSizeForPagingIo(int bufferLength, long offset, long streamLength, IDokanFileInfo info)
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
