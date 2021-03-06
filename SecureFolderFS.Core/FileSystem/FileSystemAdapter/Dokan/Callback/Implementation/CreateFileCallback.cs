using DokanNet;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Sdk.Paths;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.DataModels;
using System.Runtime.InteropServices;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths;

using FileAccess = DokanNet.FileAccess;
using static SecureFolderFS.Core.UnsafeNative.UnsafeNativeDataModels;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class CreateFileCallback : BaseDokanOperationsCallbackWithPath, ICreateFileCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public CreateFileCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            this._fileSystemOperations = fileSystemOperations;
        }

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            // ATTENTION!
            /* 
                Calls to CreateFile with info.IsDirectory set to false,
                may contain a fileName that points to a directory,
                and vice versa
            */

            // Check for wildcard filenames (filename with metacharacters)
            if (fileName.Contains('*') || fileName.Contains('?'))
            {
                // Return INVALID_NAME
                return DokanResult.InvalidName;
            }

            var currentAccess = access;
            var foInformation = GetFileObjectInformation(fileName);

            if (!foInformation.PathIsDirectory
                && (access.HasFlag(FileAccess.GenericWrite)
                    || access.HasFlag(FileAccess.WriteData)
                    || access.HasFlag(FileAccess.AppendData)))
            {
                currentAccess |= FileAccess.GenericRead; // TODO: Check if adding flags like that is safe
                if (access.HasFlag(FileAccess.AppendData))
                {
                    currentAccess |= FileAccess.WriteData;
                }
            }

            if ((!foInformation.PathIsDirectory
                    && share.HasFlag(FileShare.Write))
                || (foInformation.PathIsDirectory
                    && access.HasFlag(FileAccess.Delete)))
            {
                share |= FileShare.Read;
            }

            if (options.HasFlag((FileOptions)FILE_OPTIONS.FILE_FLAG_NO_BUFFERING))
            {
                Debugger.Break();
                options &= ~(FileOptions)FILE_OPTIONS.FILE_FLAG_NO_BUFFERING;
            }

            if (info.IsDirectory = (foInformation.PathExists ? foInformation.PathIsDirectory : info.IsDirectory))
            {
                return CreateDirectoryInternal(fileName, currentAccess, share, mode, options, attributes, foInformation, info);
            }
            else
            {
                return CreateFileInternal(fileName, access, share, mode, options, attributes, foInformation, info);
            }
        }

        private NtStatus CreateDirectoryInternal(string fileName, FileAccess currentAccess, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, FileObjectInformation foInformation, IDokanFileInfo info)
        {
            var returnStatus = DokanResult.Success;

            if (mode == FileMode.CreateNew || mode == FileMode.OpenOrCreate)
            {
                try
                {
                    if (mode == FileMode.CreateNew && foInformation.PathExists)
                    {
                        return DokanResult.AlreadyExists;
                    }
                    else if (_fileSystemOperations.DangerousDirectoryOperations.CreateDirectory(foInformation.CiphertextPath.Path) is not null)
                    {
                        // The directory has been created successfully

                        if (!_fileSystemOperations.InitializeWithDirectory(foInformation.CiphertextPath, true))
                        {
                            // The directory could not be initialized
                            returnStatus = DokanResult.PathNotFound;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    returnStatus = DokanResult.AccessDenied;
                }
                catch (IOException ioex) // TODO: Catch errors from CreateDirectory and InitializeWithDirectory
                {
                    returnStatus = GetNtStatusFromException(ioex);
                }
            }
            else if (mode == FileMode.Open)
            {
                try
                {
                    if (foInformation.PathExists)
                    {
                        // Will throw if unsuccessful and catch UnauthorizedAccessException to return DokanResult.AccessDenied
                        _ = new DirectoryInfo(foInformation.CiphertextPath.Path).EnumerateFileSystemInfos().Any(); // .Any() iterator moves by one - corresponds to FindNextFile
                    }
                    else
                    {
                        if (foInformation.PathIsDirectory)
                        {
                            return DokanResult.PathNotFound;
                        }
                        else
                        {
                            return DokanResult.FileNotFound;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    returnStatus = DokanResult.AccessDenied;
                }
            }

            try
            {
                if (returnStatus == DokanResult.Success)
                {
                    if (mode == FileMode.Open)
                    {
                        InvalidateContext(info);
                        return DokanResult.Success;
                    }

                    info.Context = handles.OpenHandleToDirectory(foInformation.CiphertextPath, mode, currentAccess, share, options);

                    if (IsContextInvalid(info))
                    {
                        int error = Marshal.GetLastWin32Error();
                        returnStatus = ToNtStatusFromWin32Error(error);
                    }
                    else
                    {
                        // TODO: Longname?
                    }

                    //if (mode == FileMode.OpenOrCreate && foInformation.ActualAttributes != (FileAttributes)Constants.FileSystem.INVALID_FILE_ATTRIBUTES)
                    //{
                    //    return NtStatus.ObjectNameCollision;
                    //}
                }
            }
            catch (IOException ioex)
            {
                returnStatus = GetNtStatusFromException(ioex);
            }

            return returnStatus;
        }

        private NtStatus CreateFileInternal(string fileName, FileAccess currentAccess, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, FileObjectInformation foInformation, IDokanFileInfo info)
        {
            var returnStatus = DokanResult.Success;

            if (foInformation.ActualAttributes != (FileAttributes)Constants.FileSystem.INVALID_FILE_ATTRIBUTES)
            {
                if ((mode == FileMode.Truncate || mode == FileMode.Create)
                    && (foInformation.ActualAttributes.HasFlag(FileAttributes.Hidden)
                        || foInformation.ActualAttributes.HasFlag(FileAttributes.System)))
                {
                    // Cannot truncate or overwrite a system or hidden file
                    return DokanResult.AccessDenied;
                }

                if (foInformation.ActualAttributes.HasFlag(FileAttributes.ReadOnly))
                {
                    if (options.HasFlag(FileOptions.DeleteOnClose))
                    {
                        // Cannot delete on close a read-only file
                        return NtStatus.CannotDelete;
                    }
                    else if (currentAccess.HasFlag(FileAccess.GenericWrite) || mode == FileMode.Truncate)
                    {
                        // Don't overwrite read-only files
                        return DokanResult.AccessDenied;
                    }
                }
            }

            if (foInformation.IsCoreFile)
            {
                returnStatus = DokanResult.Success;
                InvalidateContext(info);
            }
            else
            {
                try
                {
                    if (!foInformation.PathExists && (mode == FileMode.Truncate || mode == FileMode.Open))
                    {
                        return DokanResult.FileNotFound;
                    }
                    else if (mode == FileMode.Truncate)
                    {
                        if (!currentAccess.HasFlag(FileAccess.GenericWrite))
                        {
                            Debugger.Break();
                            return DokanResult.InvalidParameter;
                        }
                    }

                    bool readWriteAttributes = currentAccess.HasFlag(Constants.FileSystem.Dokan.DATA_ACCESS);
                    bool readAccess = currentAccess.HasFlag(Constants.FileSystem.Dokan.DATA_WRITE_ACCESS);
                    System.IO.FileAccess openAccess = readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite;
                    info.Context = handles.OpenHandleToFile(foInformation.CiphertextPath, mode, openAccess, share, options);

                    if (foInformation.PathExists && (mode == FileMode.OpenOrCreate || mode == FileMode.Create))
                    {
                        returnStatus = DokanResult.AlreadyExists;
                    }

                    if (mode == FileMode.CreateNew || mode == FileMode.Create || (!foInformation.PathExists && mode == FileMode.OpenOrCreate))
                    {
                        // File was created...
                        FileAttributes newAttributes = attributes;
                        newAttributes |= FileAttributes.Archive; // Files are always created as Archive
                                                                 // FILE_ATTRIBUTE_NORMAL is override if any other attribute is set.
                        newAttributes &= ~FileAttributes.Normal;
                        _fileSystemOperations.DangerousFileOperations.SetAttributes(foInformation.CiphertextPath.Path, newAttributes);
                    }
                }
                catch (IntegrityException)
                {
                    return NtStatus.CrcError;
                }
            }

            return returnStatus;
        }

        private FileObjectInformation GetFileObjectInformation(string fileName)
        {
            ConstructFilePath(fileName, out var cleartextPath, out var ciphertextPath);
            var isCoreFile = PathHelpers.IsCoreFile(fileName);

            var actualAttributes = (FileAttributes)Constants.FileSystem.INVALID_FILE_ATTRIBUTES;
            var pathExists = true;
            var pathIsDirectory = false;

            try
            {
                pathExists = _fileSystemOperations.DangerousFileOperations.Exists(ciphertextPath.Path) || _fileSystemOperations.DangerousDirectoryOperations.Exists(ciphertextPath.Path);

                if (pathExists)
                {
                    actualAttributes = isCoreFile ? FileAttributes.Normal : _fileSystemOperations.DangerousFileOperations.GetAttributes(ciphertextPath.Path);
                    pathIsDirectory = actualAttributes.HasFlag(FileAttributes.Directory);
                }
            }
            catch (Exception)
            {
                // File state could not be determined.
            }

            return new FileObjectInformation()
            {
                CleartextPath = cleartextPath,
                CiphertextPath = ciphertextPath,
                ActualAttributes = actualAttributes,
                IsCoreFile = isCoreFile,
                PathExists = pathExists,
                PathIsDirectory = pathIsDirectory
            };
        }
    }
}
