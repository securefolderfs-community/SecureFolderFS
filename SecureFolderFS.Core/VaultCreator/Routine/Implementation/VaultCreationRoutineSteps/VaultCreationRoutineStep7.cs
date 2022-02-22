using System;
using System.Security.Cryptography;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.VaultDataStore.VaultKeystore;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep7 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep7
    {
        public VaultCreationRoutineStep7(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep8 InitializeKeystoreData(DisposablePassword disposablePassword)
        {
            ArgumentNullException.ThrowIfNull(disposablePassword);
            ArgumentNullException.ThrowIfNull(disposablePassword.Password);

            using (disposablePassword)
            {
                using SecretKey encryptionKey = new SecretKey(new byte[Constants.Security.KeyChains.ENCRYPTIONKEY_LENGTH]); // TODO: Change to more meaningful name: DEK
                using SecretKey macKey = new SecretKey(new byte[Constants.Security.KeyChains.MACKEY_LENGTH]);
                byte[] salt = new byte[Constants.Security.KeyChains.SALT_LENGTH];

                // Fill keys
                StrongFillKeys(encryptionKey, macKey, salt);

                // Derive key-encryption-key (KEK)
                byte[] kek = vaultCreationDataModel.KeyCryptor.Argon2idCrypt.Argon2idHash(disposablePassword?.Password, salt);

                // Wrap keys
                byte[] wrappedEncryptionKey = vaultCreationDataModel.KeyCryptor.Rfc3394KeyWrap.Rfc3394WrapKey(encryptionKey, kek);
                byte[] wrappedMacKey = vaultCreationDataModel.KeyCryptor.Rfc3394KeyWrap.Rfc3394WrapKey(macKey, kek);

                vaultCreationDataModel.BaseVaultKeystore = new VaultKeystore(wrappedEncryptionKey, wrappedMacKey, salt);
                vaultCreationDataModel.MacKey = macKey.CreateCopy();

                return new VaultCreationRoutineStep8(vaultCreationDataModel);
            }
        }

        private static void StrongFillKeys(byte[] encryptionKey, byte[] macKey, byte[] salt)
        {
            using var secureRandom = RandomNumberGenerator.Create();

            secureRandom.GetNonZeroBytes(encryptionKey);
            secureRandom.GetNonZeroBytes(macKey);
            secureRandom.GetNonZeroBytes(salt);
        }
    }
}
