using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IVirtualFileSystem"/>
    internal sealed class WebDavFileSystem : IVirtualFileSystem
    {
        private readonly WebDavWrapper _webDavWrapper;

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        /// <inheritdoc/>
        public bool IsOperational { get; private set; }

        public WebDavFileSystem(IFolder rootFolder, WebDavWrapper webDavWrapper)
        {
            _webDavWrapper = webDavWrapper;

            RootFolder = rootFolder;
            IsOperational = true;
        }

        /// <inheritdoc/>
        public async Task<bool> CloseAsync(FileSystemCloseMethod closeMethod)
        {
            if (IsOperational)
            {
                var closeResult = await Task.Run(() => _webDavWrapper.CloseFileSystem(closeMethod));
                IsOperational = !closeResult;

                if (closeResult && OperatingSystem.IsWindows()) // Closed successfully
                {
                    // Close all file explorer windows
                    await CloseExplorerShellAsync(RootFolder.Id);
                }
            }

            return !IsOperational;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            _ = await CloseAsync(FileSystemCloseMethod.CloseForcefully);
        }

        private static Task CloseExplorerShellAsync(string path)
        {
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
            //            // Hook up closing event
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
            //            // Unhook to avoid leaking memory
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
