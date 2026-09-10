using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Api.Enums;

namespace SecureFolderFS.Sdk.Api.Transport
{
    /// <summary>
    /// Serves the API over a Unix domain socket restricted to the current user.
    /// </summary>
    /// <remarks>
    /// The socket file is owner-only, but the guarantee that matters is the containing directory being
    /// owner-only, which is verified before the socket is bound.
    /// </remarks>
    [UnsupportedOSPlatform("windows")]
    public sealed class UnixSocketApiTransport : IApiTransport
    {
        private const int LISTEN_BACKLOG = 16;

        private const UnixFileMode OwnerOnlyDirectory =
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

        private const UnixFileMode OwnerOnlyFile =
            UnixFileMode.UserRead | UnixFileMode.UserWrite;

        private const UnixFileMode GroupOrOtherAccess =
            UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;

        private readonly string _socketPath;
        private Socket? _listener;
        private bool _disposed;

        /// <inheritdoc/>
        public ApiTransportKind Kind => ApiTransportKind.UnixSocket;

        /// <inheritdoc/>
        public string Address => _socketPath;

        public UnixSocketApiTransport(string socketPath)
        {
            _socketPath = socketPath;
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            EnsureOwnerOnlyDirectory(Path.GetDirectoryName(_socketPath)!);
            await ClearStaleSocketAsync(cancellationToken);

            var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                listener.Bind(new UnixDomainSocketEndPoint(_socketPath));
                File.SetUnixFileMode(_socketPath, OwnerOnlyFile);
                listener.Listen(LISTEN_BACKLOG);
            }
            catch
            {
                listener.Dispose();
                throw;
            }

            _listener = listener;
        }

        /// <inheritdoc/>
        public async Task<IApiConnection> AcceptAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_listener is null)
                throw new InvalidOperationException($"{nameof(StartAsync)} must be called before accepting connections.");

            var accepted = await _listener.AcceptAsync(cancellationToken);
            return new UnixSocketConnection(accepted);
        }

        /// <summary>
        /// Creates the containing directory as owner-only, or verifies an existing one is safe to use.
        /// </summary>
        private static void EnsureOwnerOnlyDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                _ = Directory.CreateDirectory(directoryPath, OwnerOnlyDirectory);
                return;
            }

            var info = new DirectoryInfo(directoryPath);

            // A symlink here could redirect the socket somewhere world-writable
            if (info.LinkTarget is not null)
                throw new IOException($"Refusing to use '{directoryPath}' for the API socket because it is a symbolic link.");

            // A directory another user can write to would let them replace or observe the socket
            var mode = File.GetUnixFileMode(directoryPath);
            if ((mode & GroupOrOtherAccess) != 0)
                throw new IOException($"Refusing to use '{directoryPath}' for the API socket because it is accessible to users other than the owner (mode: {mode}).");
        }

        /// <summary>
        /// Removes a socket file left behind by a previous run, distinguishing a live instance from a
        /// stale file so a second instance fails instead of hijacking the endpoint.
        /// </summary>
        private async Task ClearStaleSocketAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_socketPath))
                return;

            using var probe = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                await probe.ConnectAsync(new UnixDomainSocketEndPoint(_socketPath), cancellationToken);
            }
            catch (SocketException)
            {
                // Nothing is listening, so the file is a leftover and is safe to remove.
                File.Delete(_socketPath);
                return;
            }

            throw new IOException($"Another SecureFolderFS instance is already serving the API on '{_socketPath}'.");
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            if (_disposed)
                return ValueTask.CompletedTask;

            _disposed = true;
            _listener?.Dispose();
            _listener = null;

            try
            {
                if (File.Exists(_socketPath))
                    File.Delete(_socketPath);
            }
            catch (Exception)
            {
                // A leftover socket file is recovered from on next start by ClearStaleSocketAsync
            }

            return ValueTask.CompletedTask;
        }

        private sealed class UnixSocketConnection : IApiConnection
        {
            private readonly Socket _socket;
            private readonly NetworkStream _stream;

            /// <inheritdoc/>
            public Stream Stream => _stream;

            /// <inheritdoc/>
            public ApiPeerHandle Peer { get; }

            public UnixSocketConnection(Socket socket)
            {
                _socket = socket;
                _stream = new NetworkStream(socket, ownsSocket: false);
                Peer = new ApiPeerHandle(socket);
            }

            /// <inheritdoc/>
            public async ValueTask DisposeAsync()
            {
                await _stream.DisposeAsync();
                _socket.Dispose();
            }
        }
    }
}
