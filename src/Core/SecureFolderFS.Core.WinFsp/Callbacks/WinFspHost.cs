using Fsp;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WinFsp.Callbacks
{
    internal sealed class WinFspHost : IDisposable
    {
        private readonly OnDeviceWinFsp _winFspCallbacks;
        private readonly string _startMountPoint;
        private readonly TaskCompletionSource<int> _mountTcs;
        private FileSystemHost? _host;
        private Thread? _mountThread;

        public WinFspHost(OnDeviceWinFsp winFspCallbacks, string startMountPoint)
        {
            _winFspCallbacks = winFspCallbacks;
            _startMountPoint = startMountPoint;
            _mountTcs = new TaskCompletionSource<int>();
        }

        public string? GetMountPointInternal() => _host?.MountPoint();

        public Task<IResult<int>> StartFileSystemAsync()
        {
            _mountThread = new Thread(() =>
            {
                try
                {
                    _host = new FileSystemHost(_winFspCallbacks);
                    var status = _host.Mount(_startMountPoint, null, true, 0);

                    // Signal that mount completed (success or failure)
                    _mountTcs.TrySetResult(status);
                }
                catch (Exception ex)
                {
                    _mountTcs.TrySetException(ex);
                }
            })
            {
                IsBackground = true,
                Name = "WinFsp Mount Thread"
            };

            _mountThread.Start();

            // Return task that completes when mount succeeds or fails
            return _mountTcs.Task.ContinueWith(task =>
            {
                if (task.IsFaulted)
                    return Result<int>.Failure(-1);

                var status = task.Result;
                return status < 0 ? (IResult<int>)Result<int>.Failure(status) : Result<int>.Success(status);
            });
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            try
            {
                _host?.Unmount();

                // Wait for mount thread to complete
                if (_mountThread?.IsAlive == true)
                    _mountThread.Join(TimeSpan.FromSeconds(5));

                _host?.Dispose();
                _winFspCallbacks.Dispose();
            }
            catch (Exception)
            {
                // Swallow exceptions during dispose
            }
        }
    }
}
