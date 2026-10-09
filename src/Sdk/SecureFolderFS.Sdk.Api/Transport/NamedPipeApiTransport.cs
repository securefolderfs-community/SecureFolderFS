using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Api.Enums;

namespace SecureFolderFS.Sdk.Api.Transport
{
    /// <summary>
    /// Serves the API over a Windows named pipe restricted to the current user.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class NamedPipeApiTransport : IApiTransport
    {
        private readonly string _pipeName;
        private readonly PipeSecurity _pipeSecurity;
        private NamedPipeServerStream? _pendingInstance;
        private bool _disposed;

        /// <inheritdoc/>
        public ApiTransportKind Kind => ApiTransportKind.NamedPipe;

        /// <inheritdoc/>
        public string Address => $@"\\.\pipe\{_pipeName}";

        public NamedPipeApiTransport(string pipeName)
        {
            _pipeName = pipeName;
            _pipeSecurity = CreateSecurity();
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // Create the first instance eagerly so the pipe is connectable as soon as the host starts
            _pendingInstance ??= CreateInstance();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<IApiConnection> AcceptAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var instance = _pendingInstance ?? CreateInstance();
            _pendingInstance = null;

            try
            {
                await instance.WaitForConnectionAsync(cancellationToken);
            }
            catch
            {
                await instance.DisposeAsync();
                throw;
            }

            // Stand up the next instance immediately so the pipe name never briefly disappears
            _pendingInstance = CreateInstance();

            return new NamedPipeConnection(instance);
        }

        private NamedPipeServerStream CreateInstance()
        {
            return NamedPipeServerStreamAcl.Create(
                _pipeName,
                PipeDirection.InOut,
                Constants.Limits.MAX_CONNECTIONS,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                inBufferSize: 0,
                outBufferSize: 0,
                _pipeSecurity);
        }

        private static PipeSecurity CreateSecurity()
        {
            var security = new PipeSecurity();
            var currentUser = WindowsIdentity.GetCurrent().User
                ?? throw new InvalidOperationException("Unable to determine the current user's SID.");

            // Only the user running SecureFolderFS may talk to the pipe
            security.AddAccessRule(new PipeAccessRule(
                currentUser,
                PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
                AccessControlType.Allow));

            // Named pipes are reachable remotely over SMB. Deny ACEs (ordered ahead of allow) close that
            // path for a remote logon as the same account, without affecting local clients.
            security.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.NetworkSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Deny));

            security.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.AnonymousSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Deny));

            return security;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;
            if (_pendingInstance is not null)
                await _pendingInstance.DisposeAsync();

            _pendingInstance = null;
        }

        private sealed class NamedPipeConnection : IApiConnection
        {
            private readonly NamedPipeServerStream _pipe;

            /// <inheritdoc/>
            public Stream Stream => _pipe;

            /// <inheritdoc/>
            public ApiPeerHandle Peer { get; }

            public NamedPipeConnection(NamedPipeServerStream pipe)
            {
                _pipe = pipe;
                Peer = new ApiPeerHandle(pipe.SafePipeHandle);
            }

            /// <inheritdoc/>
            public async ValueTask DisposeAsync()
            {
                try
                {
                    if (_pipe.IsConnected)
                        _pipe.Disconnect();
                }
                catch (Exception)
                {
                    // The peer may already be gone. We don't care, only dispose
                }

                await _pipe.DisposeAsync();
            }
        }
    }
}
