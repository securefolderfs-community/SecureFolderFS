using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.DataModels;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service for handling vault shortcut (.sfvault) files.
    /// </summary>
    public interface IVaultShortcutService
    {
        /// <summary>
        /// The file extension for vault shortcut files (without the leading dot).
        /// </summary>
        public const string FILE_EXTENSION = "sfvault";

        /// <summary>
        /// The file extension for vault shortcut files (with the leading dot).
        /// </summary>
        public const string FILE_EXTENSION_WITH_DOT = ".sfvault";

        /// <summary>
        /// Creates a vault shortcut data model from the specified vault information.
        /// </summary>
        /// <param name="persistableId">The persistable ID of the vault.</param>
        /// <param name="vaultName">The display name of the vault.</param>
        /// <param name="vaultPath">The full path to the vault folder.</param>
        /// <returns>A new <see cref="VaultShortcutDataModel"/> instance.</returns>
        VaultShortcutDataModel CreateShortcutData(string? persistableId, string? vaultName, string? vaultPath);

        /// <summary>
        /// Serializes the shortcut data to a stream.
        /// </summary>
        /// <param name="shortcutData">The shortcut data to serialize.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Stream"/> containing the serialized data.</returns>
        Task<Stream> SerializeAsync(VaultShortcutDataModel shortcutData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserializes shortcut data from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the serialized data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>The deserialized <see cref="VaultShortcutDataModel"/>, or null if deserialization fails.</returns>
        Task<VaultShortcutDataModel?> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads and deserializes shortcut data from a file path.
        /// </summary>
        /// <param name="filePath">The path to the .sfvault file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>The deserialized <see cref="VaultShortcutDataModel"/>, or null if reading/deserialization fails.</returns>
        Task<VaultShortcutDataModel?> ReadFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    }
}

