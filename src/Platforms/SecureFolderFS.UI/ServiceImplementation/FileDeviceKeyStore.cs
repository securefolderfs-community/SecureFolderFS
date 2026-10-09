#if APP_PLATFORM_PRESENT
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppPlatform.Services;
using SecureFolderFS.Storage.Extensions;
using static SecureFolderFS.UI.Constants.FileNames.Accounts;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <summary>
    /// File-based <see cref="IDeviceKeyStore"/>. Stores device key material for one or more accounts, each in its own subfolder under a base folder.
    /// </summary>
    public abstract class FileDeviceKeyStore : IDeviceKeyStore
    {
        private readonly IModifiableFolder _baseFolder;

        protected FileDeviceKeyStore(IModifiableFolder baseFolder)
        {
            _baseFolder = baseFolder;
        }

        /// <inheritdoc/>
        public async Task<Guid> GetOrCreateClientDeviceIdAsync(CancellationToken cancellationToken = default)
        {
            var accountsFolder = await _baseFolder.CreateFolderAsync(ACCOUNTS_FOLDER_NAME, false, cancellationToken);
            if (accountsFolder is not IModifiableFolder modifiableFolder)
                throw new InvalidOperationException("The accounts folder is not modifiable.");

            var file = await modifiableFolder.TryGetFileByNameAsync(ACCOUNT_CLIENT_DEVICE_ID_FILENAME, cancellationToken);
            if (file is not null)
            {
                // If the stored id can no longer be decrypted, TryReadProtectedTextAsync discards it
                // and returns null so a fresh id is generated below.
                var text = await TryReadProtectedTextAsync(file, modifiableFolder, cancellationToken);
                if (Guid.TryParse(text, out var existing))
                    return existing;
            }

            var idFile = await modifiableFolder.CreateFileAsync(ACCOUNT_CLIENT_DEVICE_ID_FILENAME, true, cancellationToken);
            var clientDeviceId = Guid.NewGuid();
            await WriteProtectedTextAsync(idFile, clientDeviceId.ToString(), cancellationToken);

            return clientDeviceId;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<DeviceKeyAccount>> GetAccountsAsync(CancellationToken cancellationToken = default)
        {
            var accounts = new List<DeviceKeyAccount>();
            var accountsFolder = await _baseFolder.TryGetFolderByNameAsync(ACCOUNTS_FOLDER_NAME, cancellationToken);
            if (accountsFolder is null)
                return accounts;

            await foreach (var item in accountsFolder.GetItemsAsync(StorableType.Folder, cancellationToken))
            {
                if (item is not IChildFolder accountFolder)
                    continue;

                if (await accountFolder.TryGetFirstByNameAsync(ACCOUNT_DEVICE_KEY_FILENAME, cancellationToken) is null)
                    continue;

                if (await accountFolder.TryGetFileByNameAsync(ACCOUNT_METADATA_FILENAME, cancellationToken) is IFile metaFile)
                {
                    var text = await TryReadProtectedTextAsync(metaFile, owningFolder: null, cancellationToken);
                    if (text is null)
                    {
                        // The account's metadata can no longer be decrypted with the current protection
                        // key, so the whole account is unusable. Discard it and let the user re-add it,
                        // which regenerates fresh key material rather than leaving the picker broken.
                        if (accountsFolder is IModifiableFolder modifiableAccountsFolder)
                            await TryDeleteAsync(modifiableAccountsFolder, accountFolder, cancellationToken);

                        continue;
                    }

                    var lines = text.Split(Environment.NewLine);
                    accounts.Add(new DeviceKeyAccount
                    {
                        Id = Get(lines, 0) ?? accountFolder.Name,
                        DisplayName = Get(lines, 1),
                        ServerUrl = Get(lines, 2),
                        UserId = Get(lines, 3)
                    });
                }
                else
                {
                    accounts.Add(new DeviceKeyAccount { Id = accountFolder.Name });
                }
            }

            return accounts;

            static string? Get(string[] lines, int index)
                => index < lines.Length && !string.IsNullOrEmpty(lines[index]) ? lines[index] : null;
        }

        /// <inheritdoc/>
        public async Task SetAccountAsync(DeviceKeyAccount account, CancellationToken cancellationToken = default)
        {
            var accountsFolder = await _baseFolder.CreateFolderAsync(ACCOUNTS_FOLDER_NAME, false, cancellationToken);
            if (accountsFolder is not IModifiableFolder modifiableAccountsFolder)
                throw new InvalidOperationException("The accounts folder is not modifiable.");

            var accountFolder = await modifiableAccountsFolder.CreateFolderAsync(account.Id, false, cancellationToken);
            if (accountFolder is not IModifiableFolder modifiableAccountFolder)
                throw new InvalidOperationException("The account folder is not modifiable.");

            var metadataFile = await modifiableAccountFolder.CreateFileAsync(ACCOUNT_METADATA_FILENAME, true, cancellationToken);
            var content = string.Join(Environment.NewLine,
                account.Id,
                account.DisplayName ?? string.Empty,
                account.ServerUrl ?? string.Empty,
                account.UserId ?? string.Empty);

            await WriteProtectedTextAsync(metadataFile, content, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> HasPrivateKeyAsync(string accountId, CancellationToken cancellationToken = default)
        {
            var accountsFolder = await _baseFolder.TryGetFolderByNameAsync(ACCOUNTS_FOLDER_NAME, cancellationToken);
            if (accountsFolder is null)
                return false;

            var accountFolder = await accountsFolder.TryGetFolderByNameAsync(accountId, cancellationToken);
            if (accountFolder is null || await accountFolder.TryGetFileByNameAsync(ACCOUNT_DEVICE_KEY_FILENAME, cancellationToken) is not { } file)
                return false;

            // Verify the key can still be decrypted. If the protection key no longer matches, heal by
            // discarding the stale key and reporting "no key" so the caller re-registers a fresh device
            // instead of hitting an undecryptable key later in GetPrivateKeyAsync.
            var privateKey = await TryUnprotectFileAsync(file, accountFolder as IModifiableFolder, cancellationToken);
            if (privateKey is null)
                return false;

            CryptographicOperations.ZeroMemory(privateKey);
            return true;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetPrivateKeyAsync(string accountId, CancellationToken cancellationToken = default)
        {
            var accountsFolder = await _baseFolder.TryGetFolderByNameAsync(ACCOUNTS_FOLDER_NAME, cancellationToken);
            if (accountsFolder is null)
                throw new InvalidOperationException("No device key stored. Complete App Platform setup first.");

            var accountFolder = await accountsFolder.TryGetFolderByNameAsync(accountId, cancellationToken);
            if (accountFolder is null || await accountFolder.TryGetFileByNameAsync(ACCOUNT_DEVICE_KEY_FILENAME, cancellationToken) is not { } file)
                throw new InvalidOperationException("No device key stored. Complete App Platform setup first.");

            // Discards the key if it can no longer be decrypted (returns null), surfacing the standard
            // "setup first" error so the login flow re-bootstraps the device.
            var privateKey = await TryUnprotectFileAsync(file, accountFolder as IModifiableFolder, cancellationToken);
            if (privateKey is null)
                throw new InvalidOperationException("No device key stored. Complete App Platform setup first.");

            return privateKey;
        }

        /// <inheritdoc/>
        public async Task StorePrivateKeyAsync(string accountId, byte[] privateKey, CancellationToken cancellationToken = default)
        {
            var accountsFolder = await _baseFolder.CreateFolderAsync(ACCOUNTS_FOLDER_NAME, false, cancellationToken);
            if (accountsFolder is not IModifiableFolder modifiableAccountsFolder)
                throw new InvalidOperationException("The accounts folder is not modifiable.");

            var accountFolder = await modifiableAccountsFolder.CreateFolderAsync(accountId, false, cancellationToken);
            if (accountFolder is not IModifiableFolder modifiableAccountFolder)
                throw new InvalidOperationException("The account folder is not modifiable.");

            var file = await modifiableAccountFolder.CreateFileAsync(ACCOUNT_DEVICE_KEY_FILENAME, true, cancellationToken);
            var protectedBytes = await ProtectAsync(privateKey, cancellationToken);
            await file.WriteBytesAsync(protectedBytes, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Guid?> GetDeviceIdAsync(string accountId, CancellationToken cancellationToken = default)
        {
            var accountsFolder = await _baseFolder.TryGetFolderByNameAsync(ACCOUNTS_FOLDER_NAME, cancellationToken);
            if (accountsFolder is null)
                return null;

            var accountFolder = await accountsFolder.TryGetFolderByNameAsync(accountId, cancellationToken);
            if (accountFolder is null || await accountFolder.TryGetFileByNameAsync(ACCOUNT_DEVICE_ID_FILENAME, cancellationToken) is not IFile file)
                return null;

            // If the stored id can no longer be decrypted it is discarded and treated as absent.
            var text = await TryReadProtectedTextAsync(file, accountFolder as IModifiableFolder, cancellationToken);
            return Guid.TryParse(text, out var id) ? id : null;
        }

        /// <inheritdoc/>
        public async Task StoreDeviceIdAsync(string accountId, Guid deviceId, CancellationToken cancellationToken = default)
        {
            var accountsFolder = await _baseFolder.CreateFolderAsync(ACCOUNTS_FOLDER_NAME, false, cancellationToken);
            if (accountsFolder is not IModifiableFolder modifiableAccountsFolder)
                throw new InvalidOperationException("The accounts folder is not modifiable.");

            var accountFolder = await modifiableAccountsFolder.CreateFolderAsync(accountId, false, cancellationToken);
            if (accountFolder is not IModifiableFolder modifiableAccountFolder)
                throw new InvalidOperationException("The account folder is not modifiable.");

            var file = await modifiableAccountFolder.CreateFileAsync(ACCOUNT_DEVICE_ID_FILENAME, false, cancellationToken);
            await WriteProtectedTextAsync(file, deviceId.ToString(), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task ClearAsync(string accountId, CancellationToken cancellationToken = default)
        {
            var accountsFolder = await _baseFolder.TryGetFolderByNameAsync(ACCOUNTS_FOLDER_NAME, cancellationToken);
            if (accountsFolder is not IModifiableFolder modifiableFolder)
                return;

            if (await modifiableFolder.TryGetFirstByNameAsync(accountId, cancellationToken) is { } accountFolder)
                await modifiableFolder.DeleteAsync(accountFolder, cancellationToken);
        }

        /// <summary>
        /// Reads and unprotects the text content of <paramref name="file"/>, returning <see langword="null"/>
        /// (instead of throwing) when the data can no longer be decrypted with the current protection key.
        /// </summary>
        /// <param name="file">The file to read.</param>
        /// <param name="owningFolder">The folder that contains <paramref name="file"/>, used to discard the
        /// file when it is unrecoverable. Pass <see langword="null"/> when the caller cleans up separately.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        private async Task<string?> TryReadProtectedTextAsync(IFile file, IModifiableFolder? owningFolder, CancellationToken cancellationToken)
        {
            var data = await TryUnprotectFileAsync(file, owningFolder, cancellationToken);
            return data is null ? null : Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Reads and unprotects the raw content of <paramref name="file"/>, returning <see langword="null"/>
        /// (instead of throwing) when the data can no longer be decrypted. When it cannot be decrypted the
        /// unrecoverable file is discarded so a fresh copy can be regenerated on the next write.
        /// </summary>
        private async Task<byte[]?> TryUnprotectFileAsync(IFile file, IModifiableFolder? owningFolder, CancellationToken cancellationToken)
        {
            try
            {
                var protectedBytes = await file.ReadBytesAsync(cancellationToken);
                return await UnprotectAsync(protectedBytes, cancellationToken);
            }
            catch (KeyMaterialUnrecoverableException)
            {
                if (owningFolder is not null && file is IStorableChild child)
                    await TryDeleteAsync(owningFolder, child, cancellationToken);

                return null;
            }
        }

        private static async Task TryDeleteAsync(IModifiableFolder folder, IStorableChild item, CancellationToken cancellationToken)
        {
            try
            {
                await folder.DeleteAsync(item, cancellationToken: cancellationToken);
            }
            catch (Exception)
            {
                // Best-effort cleanup; a lingering unrecoverable file is simply overwritten on the next write.
            }
        }

        private async Task WriteProtectedTextAsync(IFile file, string text, CancellationToken cancellationToken)
        {
            var protectedBytes = await ProtectAsync(Encoding.UTF8.GetBytes(text), cancellationToken);
            await file.WriteBytesAsync(protectedBytes, cancellationToken);
        }

        /// <summary>
        /// Protects sensitive data before it is written to disk.
        /// </summary>
        /// <param name="data">The data to protect.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the protected data.</returns>
        protected abstract Task<byte[]> ProtectAsync(byte[] data, CancellationToken cancellationToken);

        /// <summary>
        /// Reverses <see cref="ProtectAsync"/>, recovering the original data read from disk.
        /// </summary>
        /// <param name="data">The data to unprotect.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the unprotected data.</returns>
        /// <exception cref="KeyMaterialUnrecoverableException">
        /// Thrown when <paramref name="data"/> cannot be decrypted with the current protection key.
        /// Implementations should raise this (rather than a raw <see cref="CryptographicException"/>) so callers
        /// can discard the stale material and regenerate fresh keys instead of failing.
        /// </exception>
        protected abstract Task<byte[]> UnprotectAsync(byte[] data, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Thrown when protected key material can no longer be decrypted with the current protection key
    /// (for example after the OS secret store was reset or the fallback store was regenerated).
    /// Signals that the material is unrecoverable and should be discarded and regenerated rather than
    /// treated as a fatal error.
    /// </summary>
    public sealed class KeyMaterialUnrecoverableException : Exception
    {
        public KeyMaterialUnrecoverableException(string message)
            : base(message)
        {
        }

        public KeyMaterialUnrecoverableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
#endif
