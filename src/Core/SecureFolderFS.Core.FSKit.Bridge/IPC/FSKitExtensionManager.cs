using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FSKit.Bridge.IPC
{
    /// <summary>
    /// Manages the loading and unloading of the FSKit native extension.
    /// </summary>
    internal sealed class FSKitExtensionManager
    {
        private const string EXTENSION_IDENTIFIER = "com.securefolder.fskit.extension";
        private bool _isLoaded;
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        // P/Invoke declarations for loading system extensions
        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr objc_getClass(string className);

        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr sel_registerName(string selectorName);

        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

        public bool IsLoaded => _isLoaded;

        public async Task<bool> LoadExtensionAsync(CancellationToken cancellationToken = default)
        {
            await _loadLock.WaitAsync(cancellationToken);
            try
            {
                if (_isLoaded)
                {
                    Debug.WriteLine("FSKit extension is already loaded");
                    return true;
                }

                Debug.WriteLine($"Loading FSKit extension: {EXTENSION_IDENTIFIER}");

                // Try to load the extension using FSKit framework APIs
                // This will be called from the main Uno app which should trigger the extension loading
                var result = await LoadExtensionViaSystemAsync(cancellationToken);

                if (result)
                {
                    _isLoaded = true;
                    Debug.WriteLine("FSKit extension loaded successfully");
                }
                else
                {
                    Debug.WriteLine("Failed to load FSKit extension");
                }

                return result;
            }
            finally
            {
                _loadLock.Release();
            }
        }

        public async Task UnloadExtensionAsync()
        {
            await _loadLock.WaitAsync();
            try
            {
                if (!_isLoaded)
                {
                    Debug.WriteLine("FSKit extension is not loaded");
                    return;
                }

                Debug.WriteLine($"Unloading FSKit extension: {EXTENSION_IDENTIFIER}");

                // Extensions are typically unloaded by the system automatically
                // We just mark it as unloaded
                _isLoaded = false;

                Debug.WriteLine("FSKit extension unloaded successfully");
            }
            finally
            {
                _loadLock.Release();
            }
        }

        private async Task<bool> LoadExtensionViaSystemAsync(CancellationToken cancellationToken)
        {
            try
            {
                // For FSKit extensions, the system loads them when:
                // 1. The extension is registered with the system
                // 2. A request is made to use FSKit APIs

                // We can use FSKit's FSKitClient to activate the extension
                // This requires calling native APIs through P/Invoke or using a native helper

                // For now, we'll use a shell command approach to activate the extension
                // In production, this should use proper FSKit APIs

                var activateCommand = $"pluginkit -e use -i {EXTENSION_IDENTIFIER}";
                Debug.WriteLine($"Activating extension with command: {activateCommand}");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/pluginkit",
                    Arguments = $"-e use -i {EXTENSION_IDENTIFIER}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode == 0)
                {
                    Debug.WriteLine($"Extension activation output: {output}");

                    // Give the extension time to initialize
                    await Task.Delay(1000, cancellationToken);
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Extension activation failed with exit code {process.ExitCode}");
                    Debug.WriteLine($"Error: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading FSKit extension: {ex.Message}");
                return false;
            }
        }
    }
}

