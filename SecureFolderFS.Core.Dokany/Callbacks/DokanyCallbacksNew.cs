using DokanNet;
using SecureFolderFS.Core.Dokany.OpenHandles;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using SecureFolderFS.Core.Cryptography;
using FileAccess = DokanNet.FileAccess;

namespace SecureFolderFS.Core.Dokany.Callbacks
{
    internal class DokanyCallbacksNew : BaseDokanCallback, IDokanOperationsUnsafe
    {
        public required ISecurity Security { get; init; }

        public required IDirectoryIdAccess DirectoryIdAccess { get; init; }

        public DokanyCallbacksNew(string vaultRootPath, IPathConverter pathConverter, HandlesManager handlesManager)
            : base(vaultRootPath, pathConverter, handlesManager)
        {
        }


        /// <inheritdoc/>
        public virtual NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode,
            FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            var ciphertextPath = GetCiphertextPath(fileName);
            var result = DokanResult.Success;

            if (info.IsDirectory)
            {
                try
                {
                    switch (mode)
                    {
                        case FileMode.Open:
                            if (!Directory.Exists(ciphertextPath))
                            {
                                try
                                {
                                    if (!File.GetAttributes(ciphertextPath).HasFlag(FileAttributes.Directory))
                                        return DokanResult.NotADirectory;
                                }
                                catch (Exception)
                                {
                                    return DokanResult.FileNotFound;
                                }

                                return DokanResult.PathNotFound;
                            }

                            _ = new DirectoryInfo(ciphertextPath).EnumerateFileSystemInfos().Any(); // .Any() iterator moves by one - corresponds to FindNextFile
                            break;

                        case FileMode.CreateNew:
                            if (Directory.Exists(ciphertextPath))
                                return DokanResult.FileExists;

                            try
                            {
                                _ = File.GetAttributes(ciphertextPath).HasFlag(FileAttributes.Directory);
                                return DokanResult.AlreadyExists;
                            }
                            catch (IOException)
                            {
                            }

                            // Create directory
                            _ = Directory.CreateDirectory(ciphertextPath);

                            // Initialize directory with directory ID
                            _ = DirectoryIdAccess.SetDirectoryId(ciphertextPath, DirectoryId.CreateNew()); // TODO: Maybe nodiscard?

                            break;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return DokanResult.AccessDenied;
                }
            }
            else
            {
                var pathExists = true;
                var pathIsDirectory = false;

                var readWriteAttributes = access.HasFlag(Constants.FileSystem.Dokan.DATA_ACCESS);
                var readAccess = access.HasFlag(Constants.FileSystem.Dokan.DATA_WRITE_ACCESS);

                try
                {
                    pathExists = (Directory.Exists(ciphertextPath) || File.Exists(ciphertextPath));
                    pathIsDirectory = pathExists && File.GetAttributes(ciphertextPath).HasFlag(FileAttributes.Directory);
                }
                catch (IOException)
                {
                }

                switch (mode)
                {
                    case FileMode.Open:

                        if (pathExists)
                        {
                            // Check if driver only wants to read attributes, security info, or open directory
                            if (readWriteAttributes || pathIsDirectory)
                            {
                                if (pathIsDirectory && (access & FileAccess.Delete) == FileAccess.Delete && (access & FileAccess.Synchronize) != FileAccess.Synchronize)
                                {
                                    // It is a DeleteFile request on a directory
                                    return DokanResult.AccessDenied;
                                }

                                info.IsDirectory = pathIsDirectory;
                                InvalidateContext(info); // Must invalidate before returning DokanResult.Success

                                return DokanResult.Success;
                            }
                        }
                        else
                        {
                            return DokanResult.FileNotFound;
                        }
                        break;

                    case FileMode.CreateNew:
                        if (pathExists)
                            return DokanResult.FileExists;
                        break;

                    case FileMode.Truncate:
                        if (!pathExists)
                            return DokanResult.FileNotFound;
                        break;
                }

                try
                {
                    var openAccess = readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite;
                    info.Context = handlesManager.OpenHandleToFile(ciphertextPath, mode, openAccess, share, options);

                    if (pathExists && (mode == FileMode.OpenOrCreate || mode == FileMode.Create))
                        result = DokanResult.AlreadyExists;

                    var fileCreated = mode == FileMode.CreateNew || mode == FileMode.Create || (!pathExists && mode == FileMode.OpenOrCreate);
                    if (fileCreated)
                    {
                        var attributes2 = attributes;
                        attributes2 |= FileAttributes.Archive; // Files are always created with FileAttributes.Archive

                        // FILE_ATTRIBUTE_NORMAL is override if any other attribute is set.
                        attributes2 &= ~FileAttributes.Normal;
                        File.SetAttributes(ciphertextPath, attributes2);
                    }
                }
                catch (CryptographicException)
                {
                    // Must invalidate here, because cleanup is not called
                    // TODO: Also close ciphertextStream
                    CloseHandle(info);
                    InvalidateContext(info);
                    return NtStatus.CrcError;
                }
                catch (UnauthorizedAccessException) // Don't have access rights
                {
                    // Must invalidate here, because cleanup is not called
                    // TODO: Also close ciphertextStream
                    CloseHandle(info);
                    InvalidateContext(info);
                    return DokanResult.AccessDenied;
                }
                catch (DirectoryNotFoundException)
                {
                    return DokanResult.PathNotFound;
                }
                catch (IOException ioEx)
                {
                    var hr = (uint)Marshal.GetHRForException(ioEx);
                    switch (hr)
                    {
                        case 0x80070020: // Sharing violation
                            return DokanResult.SharingViolation;
                        default:
                            throw;
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual void Cleanup(string fileName, IDokanFileInfo info)
        {
            handlesManager.CloseHandle(GetContextValue(info));
            InvalidateContext(info);

            // Make sure we delete redirected items from DeleteDirectory() and DeleteFile() here.
            if (info.DeleteOnClose)
            {
                var ciphertextPath = GetCiphertextPath(fileName);
                try
                {
                    if (info.IsDirectory)
                    {
                        DirectoryIdAccess.RemoveDirectoryId(ciphertextPath);
                        Directory.Delete(ciphertextPath, true);
                    }
                    else
                    {
                        File.Delete(ciphertextPath);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        /// <inheritdoc/>
        public virtual void CloseFile(string fileName, IDokanFileInfo info)
        {
            _ = fileName;

            CloseHandle(info);
            InvalidateContext(info);
        }

        /// <inheritdoc/>
        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            bytesRead = 0;
            return DokanResult.NotImplemented;
        }

        /// <inheritdoc/>
        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            bytesWritten = 0;
            return DokanResult.NotImplemented;
        }

        /// <inheritdoc/>
        public virtual NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            if (handlesManager.GetHandle<FileHandle>(GetContextValue(info)) is not { } fileHandle)
                return DokanResult.InvalidHandle;

            try
            {
                fileHandle.FileStream.Flush();
                return DokanResult.Success;
            }
            catch (IOException)
            {
                return DokanResult.DiskFull;
            }
        }

        /// <inheritdoc/>
        public virtual NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            try
            {
                var ciphertextPath = GetCiphertextPath(fileName);

                FileSystemInfo finfo = new FileInfo(ciphertextPath);
                if (!finfo.Exists)
                    finfo = new DirectoryInfo(ciphertextPath);
                
                fileInfo = new FileInformation()
                {
                    FileName = finfo.Name,
                    Attributes = finfo.Attributes,
                    CreationTime = finfo.CreationTime,
                    LastAccessTime = finfo.LastAccessTime,
                    LastWriteTime = finfo.LastWriteTime,
                    Length = finfo is FileInfo fileInfo2
                        ? Security.ContentCrypt.CalculateCleartextSize(fileInfo2.Length - _security.HeaderCrypt.HeaderCiphertextSize)
                        : 0L
                };

                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                fileInfo = default;
                return DokanResult.InvalidName;
            }
            catch (FileNotFoundException)
            {
                fileInfo = default;
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                fileInfo = default;
                return DokanResult.PathNotFound;
            }
            catch (UnauthorizedAccessException)
            {
                fileInfo = default;
                return DokanResult.AccessDenied;
            }
            catch (Exception)
            {
                fileInfo = default;
                return DokanResult.Unsuccessful;
            }
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime,
            IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes,
            IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName,
            out uint maximumComponentLength, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
            IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
            IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus Mounted(string mountPoint, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset,
            IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public NtStatus WriteFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesWritten, long offset,
            IDokanFileInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
