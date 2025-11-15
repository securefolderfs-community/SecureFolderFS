using Fsp;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WinFsp.Callbacks
{
    internal sealed class WinFspService : Service, IDisposable
    {
        private readonly OnDeviceWinFsp _winFspCallbacks;
        private readonly TaskCompletionSource<int> _tcs;
        private readonly string _startMountPoint;
        private FileSystemHost? _host;
        private Thread? _fsThead;

        public WinFspService(OnDeviceWinFsp winFspCallbacks, string startMountPoint)
            : base(Constants.WinFsp.SERVICE_NAME)
        {
            _tcs = new();
            _winFspCallbacks = winFspCallbacks;
            _startMountPoint = startMountPoint;
        }

        internal string? GetMountPointInternal() => _host?.MountPoint();

        public async Task<IResult<int>> StartFileSystemAsync()
        {
            var ts = new ThreadStart(() => Run());
            _fsThead = new Thread(ts);
            _fsThead.Start();

            var status = await _tcs.Task;
            return status < 0 ? Result<int>.Failure(status) : Result<int>.Success(status);
        }

        /// <inheritdoc/>
        protected override void OnStart(string[] Args)
        {
            _host = new FileSystemHost(_winFspCallbacks);
            var status = _host.Mount(_startMountPoint, null, true);

            _tcs.TrySetResult(status);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Stop();
            _host?.Dispose();
            _winFspCallbacks.Dispose();
        }
    }
}
