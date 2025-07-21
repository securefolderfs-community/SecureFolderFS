using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.Helpers
{
    public static class VaultHelpers
    {
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
