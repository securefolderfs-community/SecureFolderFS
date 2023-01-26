using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.WebDav.Enums;

namespace SecureFolderFS.Core.WebDav.AppModels
{
    /// <inheritdoc cref="MountOptions"/>
    public sealed class WebDavMountOptions : MountOptions
    {
        /// <summary>
        /// Gets the protocol used for the connection.
        /// </summary>
        public WebDavProtocolMode Protocol { get; init; } = WebDavProtocolMode.Http;

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
