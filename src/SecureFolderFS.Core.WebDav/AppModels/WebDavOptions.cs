using SecureFolderFS.Core.WebDav.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.WebDav.AppModels
{
    /// <inheritdoc cref="FileSystemOptions"/>
    public sealed class WebDavOptions : FileSystemOptions
    {
        /// <summary>
        /// Gets the protocol used for the connection.
        /// </summary>
        public WebDavProtocolMode Protocol { get; init; } = WebDavProtocolMode.Http;

        /// <summary>
        /// Gets the domain address used for the connection.
        /// </summary>
        public string Domain { get; init; } = "localhost";

        /// <summary>
        /// Gets the port number used for the connection.
        /// </summary>
        /// <remarks>
        /// This property does not guarantee that the specified port will be used.
        /// </remarks>
        public int PreferredPort { get; init; } = 4949;
    }
}
