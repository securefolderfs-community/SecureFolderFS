using SecureFolderFS.Core.FileSystem.Models;
using SecureFolderFS.Core.WebDav.Enums;

namespace SecureFolderFS.Core.WebDav.Models
{
    /// <inheritdoc cref="MountOptions"/>
    public sealed class WebDavMountOptions : MountOptions
    {
        /// <summary>
        /// Gets the protocol used for the connection.
        /// </summary>
        public WebDavProtocol Protocol { get; init; } = WebDavProtocol.Http;

        /// <summary>
        /// Gets the domain address used for the connection.
        /// </summary>
        public required string Domain { get; init; }

        /// <summary>
        /// Gets the port number used for the connection.
        /// </summary>
        public required string Port { get; init; }
    }
}
