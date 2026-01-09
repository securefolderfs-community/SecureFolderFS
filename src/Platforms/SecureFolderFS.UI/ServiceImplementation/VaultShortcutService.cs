using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultShortcutService"/>
    public class VaultShortcutService : IVaultShortcutService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <inheritdoc/>
        public VaultShortcutDataModel CreateShortcutData(string? persistableId, string? vaultName, string? vaultPath)
        {
            return new VaultShortcutDataModel(persistableId, vaultName, vaultPath);
        }

        /// <inheritdoc/>
        public async Task<Stream> SerializeAsync(VaultShortcutDataModel shortcutData, CancellationToken cancellationToken = default)
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, shortcutData, _jsonOptions, cancellationToken);
            stream.Position = 0;
            return stream;
        }

        /// <inheritdoc/>
        public async Task<VaultShortcutDataModel?> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            try
            {
                return await JsonSerializer.DeserializeAsync<VaultShortcutDataModel>(stream, _jsonOptions, cancellationToken);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<VaultShortcutDataModel?> ReadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                await using var fileStream = File.OpenRead(filePath);
                return await DeserializeAsync(fileStream, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

