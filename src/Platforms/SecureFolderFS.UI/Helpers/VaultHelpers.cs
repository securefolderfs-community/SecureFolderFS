using System;
using System.Collections.Generic;
using System.Linq;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using static SecureFolderFS.Core.Constants.Vault;

namespace SecureFolderFS.UI.Helpers
{
    public static class VaultHelpers
    {
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
