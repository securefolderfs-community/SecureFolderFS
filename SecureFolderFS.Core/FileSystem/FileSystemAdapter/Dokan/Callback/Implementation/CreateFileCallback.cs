using DokanNet;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Paths;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FileAccess = DokanNet.FileAccess;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class CreateFileCallback : BaseDokanOperationsCallbackWithPath, ICreateFileCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public CreateFileCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathConverter pathConverter, HandlesManager handles)
            : base(vaultPath, pathConverter, handles)
        {
            _fileSystemOperations = fileSystemOperations;
        }

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
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

                            // Initialize directory with dir id
                            _fileSystemOperations.InitializeDirectory(ciphertextPath);

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
                    info.Context = handles.OpenHandleToFile(ciphertextPath, mode, openAccess, share, options);

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
                catch (IntegrityException)
                {
                    // Must invalidate here, because cleanup is not called
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
                catch (IOException ioex)
                {
                    var hr = (uint)Marshal.GetHRForException(ioex);
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
    }
}
