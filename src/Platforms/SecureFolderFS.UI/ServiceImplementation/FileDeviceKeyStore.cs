#if APP_PLATFORM_PRESENT
using System;
using System.Collections.Generic;
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
            if (file is not null && Guid.TryParse(await ReadProtectedTextAsync(file, cancellationToken), out var existing))
                return existing;

            file ??= await modifiableFolder.CreateFileAsync(ACCOUNT_CLIENT_DEVICE_ID_FILENAME, true, cancellationToken);

            var clientDeviceId = Guid.NewGuid();
            await WriteProtectedTextAsync(file, clientDeviceId.ToString(), cancellationToken);

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
                if (item is not IFolder accountFolder)
                    continue;

                if (await accountFolder.TryGetFirstByNameAsync(ACCOUNT_DEVICE_KEY_FILENAME, cancellationToken) is null)
                    continue;

                if (await accountFolder.TryGetFirstByNameAsync(ACCOUNT_METADATA_FILENAME, cancellationToken) is IFile metaFile)
                {
                    var lines = (await ReadProtectedTextAsync(metaFile, cancellationToken)).Split(Environment.NewLine);
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
            if (accountFolder is null)
                return false;

            return await accountFolder.TryGetFirstByNameAsync(ACCOUNT_DEVICE_KEY_FILENAME, cancellationToken) is not null;
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

            var protectedBytes = await file.ReadBytesAsync(cancellationToken);
            return await UnprotectAsync(protectedBytes, cancellationToken);
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
            if (accountFolder is null || await accountFolder.TryGetFirstByNameAsync(ACCOUNT_DEVICE_ID_FILENAME, cancellationToken) is not IFile file)
                return null;

            var text = await ReadProtectedTextAsync(file, cancellationToken);
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

        private async Task<string> ReadProtectedTextAsync(IFile file, CancellationToken cancellationToken)
        {
            var protectedBytes = await file.ReadBytesAsync(cancellationToken);
            var data = await UnprotectAsync(protectedBytes, cancellationToken);

            return Encoding.UTF8.GetString(data);
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
        protected abstract Task<byte[]> UnprotectAsync(byte[] data, CancellationToken cancellationToken);
    }
}
#endif
