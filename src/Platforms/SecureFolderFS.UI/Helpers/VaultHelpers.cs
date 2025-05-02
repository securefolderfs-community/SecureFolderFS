using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
                { Associations.ASSOC_FILENAME_ENCODING_ID, vaultOptions.NameEncodingId },
                { Associations.ASSOC_RECYCLE_SIZE, vaultOptions.RecycleBinSize },
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
        
        public static SecretKey GenerateChallenge(string vaultId)
        {
            var encodedVaultIdLength = Encoding.ASCII.GetByteCount(vaultId);
            using var challenge = new SecureKey(Core.Cryptography.Constants.KeyTraits.CHALLENGE_KEY_PART_LENGTH + encodedVaultIdLength);
            using var secureRandom = RandomNumberGenerator.Create();

            // Fill the first CHALLENGE_KEY_PART_LENGTH bytes with secure random data
            secureRandom.GetNonZeroBytes(challenge.Key.AsSpan(0, Core.Cryptography.Constants.KeyTraits.CHALLENGE_KEY_PART_LENGTH));

            // Fill the remaining bytes with the ID
            // By using ASCII encoding we get 1:1 byte to char ratio which allows us
            // to use the length of the string ID as part of the SecretKey length above
            var written = Encoding.ASCII.GetBytes(vaultId, challenge.Key.AsSpan(Core.Cryptography.Constants.KeyTraits.CHALLENGE_KEY_PART_LENGTH));
            if (written != encodedVaultIdLength)
                throw new FormatException("The allocated buffer and vault ID written bytes amount were different.");

            // Return a copy of the challenge since the original version is being disposed of
            return challenge.CreateCopy();
        }
    }
}
