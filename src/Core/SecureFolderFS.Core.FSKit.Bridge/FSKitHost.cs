using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FSKit.Bridge.IPC;
using SecureFolderFS.Sdk.Ipc.Extensions;

namespace SecureFolderFS.Core.FSKit.Bridge
{
    /// <summary>
    /// Manages the FSKit file system lifecycle via IPC communication.
    /// </summary>
    internal sealed class FSKitHost : IDisposable
    {
        private readonly string _mountPoint;
        private readonly string _volumeName;
        private readonly bool _isReadOnly;
        private readonly FSKitProcessManager _processManager;
        private readonly FSKitIPCClient _ipcClient;
        private bool _isMounted;
        private bool _isDisposed;

        public bool IsMounted => _isMounted && !_isDisposed;

        public FSKitHost(string mountPoint, string volumeName, bool isReadOnly)
        {
            _mountPoint = mountPoint ?? throw new ArgumentNullException(nameof(mountPoint));
            _volumeName = volumeName ?? throw new ArgumentNullException(nameof(volumeName));
            _isReadOnly = isReadOnly;
            _processManager = new FSKitProcessManager();
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
                // Step 1: Start the FSKit service process
                Debug.WriteLine("FSKit: Starting FSKit service...");
                var serviceStarted = await _processManager.StartServiceAsync(cancellationToken);
                if (!serviceStarted)
                {
                    Debug.WriteLine("FSKit: Failed to start FSKit service process");
                    return false;
                }

                // Step 2: Connect to the IPC server
                Debug.WriteLine("FSKit: Connecting to IPC server...");
                var connected = await _ipcClient.ConnectAsync(retries: 15, delayMs: 500, cancellationToken);
                if (!connected)
                {
                    Debug.WriteLine("FSKit: Failed to connect to IPC server");
                    await _processManager.StopServiceAsync();
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
                    await _processManager.StopServiceAsync();
                    return false;
                }

                // Step 4: Send mount request
                Debug.WriteLine($"FSKit: Mounting file system at {_mountPoint}...");
                var mountRequest = MessageExtensions.CreateMountRequest(_mountPoint, _volumeName, _isReadOnly);
                var mountResponse = await _ipcClient.SendRequestAsync(mountRequest, cancellationToken);

                if (!mountResponse.IsSuccess())
                {
                    Debug.WriteLine($"FSKit: Mount failed - {mountResponse.Error ?? mountResponse.Message}");
                    _ipcClient.Disconnect();
                    await _processManager.StopServiceAsync();
                    return false;
                }

                _isMounted = true;
                Debug.WriteLine($"FSKit: File system successfully mounted at {_mountPoint}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FSKit: Error during mount: {ex.Message}");

                // Cleanup on error
                try
                {
                    _ipcClient.Disconnect();
                    await _processManager.StopServiceAsync();
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
                if (_ipcClient.IsConnected)
                {
                    Debug.WriteLine($"FSKit: Unmounting file system from {_mountPoint}...");

                    try
                    {
                        var unmountRequest = MessageExtensions.CreateUnmountRequest(_mountPoint);
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
            }
            finally
            {
                // Step 2: Disconnect IPC and stop service
                _ipcClient.Disconnect();
                await _processManager.StopServiceAsync();
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
                _processManager.Dispose();
            }
        }
    }
}

