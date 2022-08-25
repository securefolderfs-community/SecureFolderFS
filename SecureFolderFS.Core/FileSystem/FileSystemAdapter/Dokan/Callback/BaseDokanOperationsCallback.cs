using System;
using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.UnsafeNative;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback
{
    internal abstract class BaseDokanOperationsCallback : IDisposable
    {
        private bool _disposed;

        protected readonly HandlesManager handles;

        protected BaseDokanOperationsCallback(HandlesManager handles)
        {
            this.handles = handles;
        }

        protected void CloseHandle(IDokanFileInfo info)
        {
            handles.CloseHandle(GetContextValue(info));
        }

        protected static bool IsContextInvalid(IDokanFileInfo info)
        {
            return GetContextValue(info) == Constants.FileSystem.INVALID_HANDLE;
        }

        protected static void InvalidateContext(IDokanFileInfo info)
        {
            info.Context = Constants.FileSystem.INVALID_HANDLE;
        }

        protected static long GetContextValue(IDokanFileInfo info)
        {
            if (info?.Context is null)
            {
                return Constants.FileSystem.INVALID_HANDLE;
            }
            else
            {
                return (long)info.Context;
            }
        }

        protected static NtStatus ToNtStatusFromWin32Error(long error)
        {
            return (NtStatus)UnsafeNativeApis.DokanNtStatusFromWin32((uint)error);
        }

        protected static NtStatus GetNtStatusFromException(Exception exception)
        {
            if (!exception.Win32ErrorFromException(out var win32error))
            {
                return DokanResult.InternalError;
            }
            else
            {
                return ToNtStatusFromWin32Error(win32error);
            }
        }

        protected void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                handles.Dispose();
            }
        }
    }
}
