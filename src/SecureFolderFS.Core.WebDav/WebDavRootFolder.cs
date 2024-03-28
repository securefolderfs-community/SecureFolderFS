using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IVFSRootFolder"/>
    internal sealed class WebDavRootFolder : WrappedFileSystemFolder
    {
        private readonly WebDavWrapper _webDavWrapper;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = "WebDav";

        public WebDavRootFolder(WebDavWrapper webDavWrapper, IFolder storageRoot, IReadWriteStatistics readWriteStatistics)
            : base(storageRoot, readWriteStatistics)
        {
            _webDavWrapper = webDavWrapper;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = await Task.Run(() => _webDavWrapper.CloseFileSystem(FileSystemCloseMethod.CloseForcefully));
            if (_disposed && OperatingSystem.IsWindows()) // Closed successfully
            {
                // Close all file explorer windows
                await CloseExplorerShellAsync(Inner.Id);
            }
        }

        private static Task CloseExplorerShellAsync(string path)
        {
            _ = path;
            return Task.CompletedTask;
            //try
            //{
            //    var formattedPath = PathHelpers.EnsureNoTrailingPathSeparator(path);
            //    var shellWindows = new SHDocVw.ShellWindows();

            //    foreach (SHDocVw.InternetExplorer ie in shellWindows)
            //    {
            //        var formattedName = Path.GetFileNameWithoutExtension(ie.FullName);
            //        if (!formattedName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
            //            continue;

            //        var url = ie.LocationURL.Replace('/', Path.DirectorySeparatorChar);
            //        var formattedUrl = Uri.UnescapeDataString(url);
            //        if (!formattedUrl.Contains(formattedPath))
            //            continue;

            //        var windowClosed = false;
            //        try
            //        {
            //            // Attach closing event
            //            ie.WindowClosing += Window_Closing;

            //            // Try quit first
            //            ie.Quit();

            //            // Wait a short delay
            //            await Task.Delay(100);

            //            if (!windowClosed)
            //            {
            //                // Retry with WM_CLOSE
            //                var hWnd = new IntPtr(ie.HWND);
            //                _ = UnsafeNativeApis.SendMessageA(hWnd, WindowMessages.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            // May sometimes throw when trying to access invalid window handle
            //            _ = ex;
            //        }
            //        finally
            //        {
            //            // Detach to avoid leaking memory
            //            ie.WindowClosing -= Window_Closing;
            //        }

            //        void Window_Closing(bool IsChildWindow, ref bool Cancel)
            //        {
            //            windowClosed = true;
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    // Something went terribly wrong
            //    Debugger.Break();
            //}
        }
    }
}
