using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Storage.VirtualFileSystem;

#if WINDOWS
using System;
using System.Diagnostics;
using System.IO;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Uno.UnsafeNative;
#endif

namespace SecureFolderFS.Uno.Platforms.Windows
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class WindowsWebDavVFSRoot : VFSRoot
    {
        private const uint WM_CLOSE = 0x0010;

        private readonly WebDavWrapper _webDavWrapper;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Core.WebDav.Constants.FileSystem.FS_NAME;

        public WindowsWebDavVFSRoot(WebDavWrapper webDavWrapper, IFolder storageRoot, FileSystemOptions options)
            : base(storageRoot, options)
        {
            _webDavWrapper = webDavWrapper;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = await _webDavWrapper.CloseFileSystemAsync();
            if (_disposed)
            {
                FileSystemManager.Instance.RemoveRoot(this);
                await CloseExplorerShellAsync(Inner.Id);
            }
        }

        private static async Task CloseExplorerShellAsync(string path)
        {
#if WINDOWS
            try
            {
                var formattedPath = PathHelpers.EnsureNoTrailingPathSeparator(path);
                var shellWindows = new SHDocVw.ShellWindows();

                foreach (SHDocVw.InternetExplorer ie in shellWindows)
                {
                    var formattedName = Path.GetFileNameWithoutExtension(ie.FullName);
                    if (!formattedName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var url = ie.LocationURL.Replace('/', Path.DirectorySeparatorChar);
                    var formattedUrl = Uri.UnescapeDataString(url);
                    if (!formattedUrl.Contains(formattedPath))
                        continue;

                    var windowClosed = false;
                    try
                    {
                        // Attach closing event
                        ie.WindowClosing += Window_Closing;

                        // Try quit first
                        ie.Quit();

                        // Wait a short delay
                        await Task.Delay(100);

                        if (!windowClosed)
                        {
                            // Retry with WM_CLOSE
                            var hWnd = new IntPtr(ie.HWND);
                            _ = UnsafeNativeApis.SendMessageA(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                    catch (Exception ex)
                    {
                        // May sometimes throw when trying to access invalid window handle
                        _ = ex;
                    }
                    finally
                    {
                        // Detach to avoid leaking memory
                        ie.WindowClosing -= Window_Closing;
                    }
                    continue;

                    void Window_Closing(bool IsChildWindow, ref bool Cancel)
                    {
                        windowClosed = true;
                    }
                }
            }
            catch (Exception)
            {
                // Something went terribly wrong
                Debugger.Break();
            }
#else
            _ = path;
            await Task.CompletedTask;
#endif
        }
    }
}