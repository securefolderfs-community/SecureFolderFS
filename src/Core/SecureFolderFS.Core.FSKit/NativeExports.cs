using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SecureFolderFS.Core.FSKit.Ipc;

namespace SecureFolderFS.Core.FSKit
{
    /// <summary>
    /// Provides native entry points for the FSKit extension to call from Objective-C/Swift.
    /// These methods are exported and callable from the native FSKit extension.
    /// </summary>
    public class NativeExports
    {
        private static FSKitIPCServer? _ipcServer;
        private static bool _isInitialized;

        [UnmanagedCallersOnly(EntryPoint = "SecureFolderFS_FSKit_GetStatusErr")]
        public static int GetStatusErr()
        {
            return 10;
        }

        /// <summary>
        /// Initializes the FSKit .NET library and starts the IPC server.
        /// Called when the native extension loads.
        /// </summary>
        /// <returns>0 on success, non-zero on failure</returns>
        [UnmanagedCallersOnly(EntryPoint = "FSKit_Initialize")]
        public static int Initialize()
        {
            try
            {
                if (_isInitialized)
                {
                    Console.WriteLine("FSKit: Already initialized");
                    return 0;
                }

                Console.WriteLine("FSKit: Initializing .NET library...");

                // Start the IPC server
                _ipcServer = new FSKitIPCServer();

                // Start in background task
                Task.Run(async () =>
                {
                    try
                    {
                        await _ipcServer.StartAsync();
                        Console.WriteLine("FSKit: IPC server started successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"FSKit: Failed to start IPC server: {ex.Message}");
                    }
                });

                _isInitialized = true;
                Console.WriteLine("FSKit: Initialization complete");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSKit: Initialization failed: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Shuts down the FSKit .NET library and stops the IPC server.
        /// Called when the native extension unloads or app terminates.
        /// </summary>
        /// <returns>0 on success, non-zero on failure</returns>
        [UnmanagedCallersOnly(EntryPoint = "FSKit_Shutdown")]
        public static int Shutdown()
        {
            try
            {
                if (!_isInitialized)
                {
                    Console.WriteLine("FSKit: Not initialized");
                    return 0;
                }

                Console.WriteLine("FSKit: Shutting down .NET library...");

                // Stop the IPC server
                if (_ipcServer != null)
                {
                    _ipcServer.StopAsync().GetAwaiter().GetResult();
                    _ipcServer = null;
                }

                _isInitialized = false;
                Console.WriteLine("FSKit: Shutdown complete");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSKit: Shutdown failed: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Gets the current status of the FSKit library.
        /// </summary>
        /// <returns>1 if initialized and running, 0 otherwise</returns>
        [UnmanagedCallersOnly(EntryPoint = "FSKit_GetStatus")]
        public static int GetStatus()
        {
            return _isInitialized && _ipcServer != null ? 1 : 0;
        }
    }
}

