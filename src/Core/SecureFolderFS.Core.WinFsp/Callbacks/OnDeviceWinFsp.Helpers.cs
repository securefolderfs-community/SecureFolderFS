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

            // If the caller can write data, append write access
            if ((rights & (FileSystemRights.WriteData | FileSystemRights.AppendData |
                           FileSystemRights.CreateFiles | FileSystemRights.CreateDirectories)) != 0)
                access |= FileAccess.Write;

            // If the caller can only read
            if ((rights & (FileSystemRights.ReadData | FileSystemRights.ReadAttributes |
                           FileSystemRights.ReadExtendedAttributes)) != 0)
                access |= FileAccess.Read;

            return access;
        }

        private static FileOptions ToFileOptions(uint createOptions)
        {
            var options = FileOptions.None;

            if ((createOptions & 0x00000002) != 0) // FILE_WRITE_THROUGH
                options |= FileOptions.WriteThrough;

            if ((createOptions & 0x00000004) != 0) // FILE_SEQUENTIAL_ONLY
                options |= FileOptions.SequentialScan;

            if ((createOptions & 0x00000800) != 0) // FILE_RANDOM_ACCESS
                options |= FileOptions.RandomAccess;

            if ((createOptions & 0x00001000) != 0) // FILE_DELETE_ON_CLOSE
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
