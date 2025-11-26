using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FSKit.Bridge.IPC;
using SecureFolderFS.Sdk.Ipc.Extensions;

namespace SecureFolderFS.Core.FSKit.Bridge
{
    /// <summary>
    /// Manages the FSKit file system lifecycle via IPC communication with the native extension.
    /// </summary>
    internal sealed class FSKitHost : IDisposable
    {
        private readonly string _volumeName;
        private readonly bool _isReadOnly;
        private readonly FSKitExtensionManager _extensionManager;
        private readonly FSKitIPCClient _ipcClient;
        private bool _isMounted;
        private bool _isDisposed;
        private string? _actualMountPoint; // Determined by FSKit after mounting

        public bool IsMounted => _isMounted && !_isDisposed;

        /// <summary>
        /// Gets the actual mount point assigned by FSKit after successful mounting.
        /// </summary>
        public string? MountPoint => _actualMountPoint;

        public FSKitHost(string volumeName, bool isReadOnly)
        {
            _volumeName = volumeName ?? throw new ArgumentNullException(nameof(volumeName));
            _isReadOnly = isReadOnly;
            _extensionManager = new FSKitExtensionManager();
            _ipcClient = new FSKitIPCClient();
        }

        public async Task<bool> StartFileSystemAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(FSKitHost));

            if (_isMounted)
            {
                Debug.WriteLine("FSKit: File system is already mounted");
                return true;
            }

            try
            {
                // Step 1: Load the FSKit extension (this loads the native extension which loads our .NET library)
                Debug.WriteLine("FSKit: Loading FSKit extension...");
                var extensionLoaded = await _extensionManager.LoadExtensionAsync(cancellationToken);
                if (!extensionLoaded)
                {
                    Debug.WriteLine("FSKit: Failed to load FSKit extension");
                    return false;
                }

                // Step 2: Connect to the IPC server (started by the .NET library when loaded)
                Debug.WriteLine("FSKit: Connecting to IPC server...");
                var connected = await _ipcClient.ConnectAsync(retries: 15, delayMs: 500, cancellationToken);
                if (!connected)
                {
                    Debug.WriteLine("FSKit: Failed to connect to IPC server");
                    await _extensionManager.UnloadExtensionAsync();
                    return false;
                }

                // Step 3: Send ping to verify connection
                Debug.WriteLine("FSKit: Verifying IPC connection...");
                var pingRequest = MessageExtensions.CreatePingRequest();
                var pingResponse = await _ipcClient.SendRequestAsync(pingRequest, cancellationToken);
                if (!pingResponse.IsSuccess())
                {
                    Debug.WriteLine($"FSKit: Ping failed - {pingResponse.Error}");
                    _ipcClient.Disconnect();
                    await _extensionManager.UnloadExtensionAsync();
                    return false;
                }

                // Step 4: Send mount request (mount point will be determined by FSKit)
                Debug.WriteLine($"FSKit: Mounting file system with volume name '{_volumeName}'...");
                var mountRequest = MessageExtensions.CreateMountRequest(null, _volumeName, _isReadOnly);
                var mountResponse = await _ipcClient.SendRequestAsync(mountRequest, cancellationToken);

                if (!mountResponse.IsSuccess())
                {
                    Debug.WriteLine($"FSKit: Mount failed - {mountResponse.Error ?? mountResponse.Message}");
                    _ipcClient.Disconnect();
                    await _extensionManager.UnloadExtensionAsync();
                    return false;
                }

                // Extract the actual mount point assigned by FSKit
                if (mountResponse.Data?.TryGetValue("mountPoint", out var mountPointObj) == true && mountPointObj != null)
                {
                    _actualMountPoint = mountPointObj.ToString();
                    Debug.WriteLine($"FSKit: File system successfully mounted at {_actualMountPoint}");
                }
                else
                {
                    Debug.WriteLine("FSKit: File system mounted but mount point not returned");
                }

                _isMounted = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FSKit: Error during mount: {ex.Message}");

                // Cleanup on error
                try
                {
                    _ipcClient.Disconnect();
                    await _extensionManager.UnloadExtensionAsync();
                }
                catch { /* Ignore cleanup errors */ }

                return false;
            }
        }

        public async Task StopFileSystemAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                return;

            if (!_isMounted)
            {
                Debug.WriteLine("FSKit: File system is not mounted");
                return;
            }

            try
            {
                // Step 1: Send unmount request via IPC
                if (_ipcClient.IsConnected && _actualMountPoint != null)
                {
                    Debug.WriteLine($"FSKit: Unmounting file system from {_actualMountPoint}...");

                    try
                    {
                        var unmountRequest = MessageExtensions.CreateUnmountRequest(_actualMountPoint);
                        var unmountResponse = await _ipcClient.SendRequestAsync(unmountRequest, cancellationToken);

                        if (!unmountResponse.IsSuccess())
                        {
                            Debug.WriteLine($"FSKit: Unmount warning - {unmountResponse.Error ?? unmountResponse.Message}");
                        }
                        else
                        {
                            Debug.WriteLine("FSKit: File system unmounted successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"FSKit: Error during unmount: {ex.Message}");
                    }
                }

                _isMounted = false;
                _actualMountPoint = null;
            }
            finally
            {
                // Step 2: Disconnect IPC and unload extension
                _ipcClient.Disconnect();
                await _extensionManager.UnloadExtensionAsync();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            try
            {
                StopFileSystemAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                _isDisposed = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FSKit: Error during dispose: {ex.Message}");
            }
            finally
            {
                _ipcClient.Dispose();
            }
        }
    }
}

