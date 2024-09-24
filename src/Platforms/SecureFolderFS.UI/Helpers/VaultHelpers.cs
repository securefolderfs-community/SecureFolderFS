using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using static SecureFolderFS.Core.Constants.Vault;

namespace SecureFolderFS.UI.Helpers
{
    public static class VaultHelpers
    {
        public static async Task<string> GetBestFileSystemAsync(CancellationToken cancellationToken)
        {
            var vaultService = DI.Service<IVaultService>();
            var settingsService = DI.Service<ISettingsService>();

            string? lastBestId = null;
            foreach (var item in vaultService.GetFileSystems())
            {
                if (item.Id == settingsService.UserSettings.PreferredFileSystemId)
                {
                    if ((await item.GetStatusAsync(cancellationToken)).Successful)
                        return item.Id;
                }
                else
                {
                    if (lastBestId is null && (await item.GetStatusAsync(cancellationToken)).Successful)
                        lastBestId = item.Id;
                }
            }

            if (lastBestId is null)
                throw new NotSupportedException("No supported adapters found.");

            return lastBestId;
        }

        public static IDictionary<string, object?> ParseOptions(VaultOptions vaultOptions)
        {
            return new Dictionary<string, object?>()
            {
                { Associations.ASSOC_CONTENT_CIPHER_ID, vaultOptions.ContentCipherId },
                { Associations.ASSOC_FILENAME_CIPHER_ID, vaultOptions.FileNameCipherId },
                { Associations.ASSOC_AUTHENTICATION, string.Join(Authentication.SEPARATOR, vaultOptions.AuthenticationMethod) },
                { Associations.ASSOC_VERSION, vaultOptions.Version },
                { Associations.ASSOC_VAULT_ID, vaultOptions.VaultId }
            };
        }

        public static SecretKey ParsePasskeySecret(IKey passkey)
        {
            var keyArray = passkey.ToArray();
            var secretKey = new SecureKey(keyArray.Length);

            keyArray.CopyTo(secretKey.Key, 0);
            Array.Clear(keyArray);

            return secretKey;
        }
    }
}
