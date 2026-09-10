using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using SecureFolderFS.Sdk.Api.Enums;

namespace SecureFolderFS.Sdk.Api.Transport
{
    /// <summary>
    /// Carries the platform handles needed to identify the process on the other end of a connection.
    /// </summary>
    public sealed class ApiPeerHandle
    {
        /// <summary>
        /// Gets the transport that produced this peer.
        /// </summary>
        public ApiTransportKind Kind { get; }

        /// <summary>
        /// Gets the server-side pipe handle, when the transport is a named pipe.
        /// </summary>
        public SafePipeHandle? PipeHandle { get; }

        /// <summary>
        /// Gets the accepted socket, when the transport is a Unix domain socket.
        /// </summary>
        public Socket? Socket { get; }

        public ApiPeerHandle(SafePipeHandle pipeHandle)
        {
            Kind = ApiTransportKind.NamedPipe;
            PipeHandle = pipeHandle;
        }

        public ApiPeerHandle(Socket socket)
        {
            Kind = ApiTransportKind.UnixSocket;
            Socket = socket;
        }
    }

    /// <summary>
    /// One accepted client connection.
    /// </summary>
    public interface IApiConnection : IAsyncDisposable
    {
        /// <summary>
        /// Gets the duplex byte stream carrying newline-delimited JSON.
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// Gets the handles used to gather evidence about the connecting process.
        /// </summary>
        ApiPeerHandle Peer { get; }
    }

    /// <summary>
    /// Listens for local client connections on one platform IPC mechanism.
    /// </summary>
    public interface IApiTransport : IAsyncDisposable
    {
        /// <summary>
        /// Gets the transport mechanism this instance implements.
        /// </summary>
        ApiTransportKind Kind { get; }

        /// <summary>
        /// Gets the address clients connect to, as advertised in the endpoint file.
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Binds the endpoint and begins listening.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Waits for the next client to connect.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the accepted connection.</returns>
        Task<IApiConnection> AcceptAsync(CancellationToken cancellationToken = default);
    }
}
