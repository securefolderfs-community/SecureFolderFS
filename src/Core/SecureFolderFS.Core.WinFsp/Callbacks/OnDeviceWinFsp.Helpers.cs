using SecureFolderFS.Core.FileSystem.Helpers.Paths.Native;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace SecureFolderFS.Core.WinFsp.Callbacks
{
    public sealed partial class OnDeviceWinFsp
    {
        private string GetCiphertextPath(string plaintextName)
        {
            return NativePathHelpers.GetCiphertextPath(plaintextName, _specifics);
        }

        private static bool IsDirectory(string ciphertextPath)
        {
            return Directory.Exists(ciphertextPath) && !File.Exists(ciphertextPath);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InvalidateContext(out object FileDesc)
        {
            FileDesc = FileSystem.Constants.INVALID_HANDLE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsContextInvalid(object FileDesc)
        {
            return FileDesc is not ulong ctxUlong || ctxUlong == FileSystem.Constants.INVALID_HANDLE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong GetContextValue(object FileDesc)
        {
            return FileDesc is ulong ctxUlong ? ctxUlong : FileSystem.Constants.INVALID_HANDLE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CloseHandle(object FileDesc)
        {
            _handlesManager.CloseHandle(GetContextValue(FileDesc));
        }

        private static FileAccess ToFileAccess(FileSystemRights rights)
        {
            var access = FileAccess.Read;

            // Check for write permissions
            var writeRights = FileSystemRights.WriteData | FileSystemRights.AppendData |
                              FileSystemRights.CreateFiles | FileSystemRights.CreateDirectories;

            if ((rights & writeRights) != 0)
                access = FileAccess.ReadWrite;
            else if ((rights & (FileSystemRights.ReadData | FileSystemRights.ReadAttributes)) == 0)
                access = 0; // No access

            return access == 0 ? FileAccess.Read : access;
        }

        private static FileOptions ToFileOptions(uint createOptions)
        {
            var options = FileOptions.None;

            if ((createOptions & FILE_WRITE_THROUGH) != 0)
                options |= FileOptions.WriteThrough;

            if ((createOptions & FILE_SEQUENTIAL_ONLY) != 0)
                options |= FileOptions.SequentialScan;

            if ((createOptions & FILE_RANDOM_ACCESS) != 0)
                options |= FileOptions.RandomAccess;

            if ((createOptions & FILE_DELETE_ON_CLOSE) != 0)
                options |= FileOptions.DeleteOnClose;

            return options;
        }

        private static int Trace(int status, string? fileName = null, [CallerMemberName] string methodName = "")
        {
#if !DEBUG
            return status;
#endif
            if (!Core.FileSystem.Constants.OPT_IN_FOR_OPTIONAL_DEBUG_TRACING)
                return status;

            if (!Debugger.IsAttached)
                return status;

            var message = $"{methodName}('{fileName}') -> {status}";
            Debug.WriteLine(message);

            return status;
        }
    }
}
