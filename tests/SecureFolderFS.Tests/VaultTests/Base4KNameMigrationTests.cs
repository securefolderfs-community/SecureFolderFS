using System.Security.Cryptography;
using FluentAssertions;
using Lex4K;
using NUnit.Framework;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Tests.Helpers;
using FileSystemNames = SecureFolderFS.Core.FileSystem.Constants.Names;
using VaultNames = SecureFolderFS.Core.Constants.Vault.Names;

namespace SecureFolderFS.Tests.VaultTests
{
    /// <summary>
    /// Covers the re-encoding of Base4K ciphertext names during the V3 to V4 migration.
    /// </summary>
    /// <remarks>
    /// The Base4K implementation was swapped from Lex4K to Secomba's after V3 was released. The two are
    /// mutually unreadable, so a Base4K vault whose names were carried over untouched would mount empty.
    /// </remarks>
    [TestFixture]
    public class Base4KNameMigrationTests
    {
        /// <summary>
        /// The whole conversion rests on being able to tell the two encodings apart by inspection alone,
        /// which is what makes it idempotent and safe to repeat after an interrupted run.
        /// </summary>
        [Test]
        public void Base4K_LegacyAndCurrentEncodings_AreMutuallyUnreadable()
        {
            for (var length = 17; length < 96; length++)
            {
                var raw = RandomNumberGenerator.GetBytes(length);
                var legacyEncoded = Base4K.EncodeChainToString(raw);
                var currentEncoded = SecombaBase4K.Encode(raw);

                legacyEncoded.Should().NotBe(currentEncoded);

                // A legacy name must never look like an already-converted one, or it would be skipped
                SecombaBase4K.Decode(legacyEncoded).Should().BeNull();

                // A converted name must never look like a legacy one, or it would be converted twice
                var decodedAsLegacy = TryDecodeLegacy(currentEncoded);
                (decodedAsLegacy is null || Base4K.EncodeChainToString(decodedAsLegacy) != currentEncoded).Should().BeTrue();
            }
        }

        [Test]
        public async Task Create_V3Vault_WithBase4KNames_MigrateTo_V4Vault_ReencodesNames()
        {
            // Arrange
            var (vaultFolder, recoveryKey) = await MockVaultHelpers.CreateVaultV3Async(null);
            using var security = CreateSecurity(recoveryKey);
            var contentFolder = (IModifiableFolder)await vaultFolder.GetFolderByNameAsync(VaultNames.VAULT_CONTENT_FOLDERNAME);

            // A file at the content root, where no Directory ID applies
            var expectedFileName = EncryptName(security, "hello.txt", []);
            await contentFolder.CreateFileAsync(ToLegacyName(expectedFileName), false);

            // A folder carrying a Directory ID, holding a file encrypted against that ID
            var expectedFolderName = EncryptName(security, "documents", []);
            var childFolder = (IModifiableFolder)await contentFolder.CreateFolderAsync(ToLegacyName(expectedFolderName), false);

            var directoryId = RandomNumberGenerator.GetBytes(16);
            var directoryIdFile = await childFolder.CreateFileAsync(FileSystemNames.DIRECTORY_ID_FILENAME, false);
            await using (var directoryIdStream = await directoryIdFile.OpenWriteAsync())
                await directoryIdStream.WriteAsync(directoryId);

            var expectedNestedName = EncryptName(security, "nested.bin", directoryId);
            await childFolder.CreateFileAsync(ToLegacyName(expectedNestedName), false);

            // Act
            await MigrateAsync(vaultFolder);

            // Assert
            var rootNames = await GetItemNamesAsync(contentFolder);
            rootNames.Should().BeEquivalentTo([
                expectedFileName + FileSystemNames.ENCRYPTED_FILE_EXTENSION,
                expectedFolderName + FileSystemNames.ENCRYPTED_FILE_EXTENSION
            ]);

            var migratedFolder = await contentFolder.GetFolderByNameAsync(expectedFolderName + FileSystemNames.ENCRYPTED_FILE_EXTENSION);
            var childNames = await GetItemNamesAsync(migratedFolder);
            childNames.Should().BeEquivalentTo([
                FileSystemNames.DIRECTORY_ID_FILENAME,
                expectedNestedName + FileSystemNames.ENCRYPTED_FILE_EXTENSION
            ]);

            // The Directory ID is raw key material, not an encoded name, and must be carried over as-is
            var migratedDirectoryIdFile = await migratedFolder.GetFileByNameAsync(FileSystemNames.DIRECTORY_ID_FILENAME);
            await using (var migratedDirectoryIdStream = await migratedDirectoryIdFile.OpenReadAsync())
            {
                var buffer = new byte[directoryId.Length];
                _ = await migratedDirectoryIdStream.ReadAtLeastAsync(buffer, buffer.Length, false);
                buffer.Should().Equal(directoryId);
            }

            // The names must be readable by the current implementation, which is the point of the exercise
            security.NameCrypt!.DecryptName(expectedFileName, []).Should().Be("hello.txt");
            security.NameCrypt!.DecryptName(expectedFolderName, []).Should().Be("documents");
            security.NameCrypt!.DecryptName(expectedNestedName, directoryId).Should().Be("nested.bin");
        }

        private static async Task MigrateAsync(IFolder vaultFolder)
        {
            var service = DI.Service<IVaultService>();
            using var migrator = await service.GetMigratorAsync(vaultFolder);
            using var keySequence = new KeySequence();
            keySequence.Add(new DisposablePassword(MockVaultHelpers.VAULT_PASSWORD));

            var contract = await migrator.UnlockAsync(keySequence);
            await migrator.MigrateAsync(contract, new());
        }

        private static Security CreateSecurity(string recoveryKey)
        {
            using var combinedKey = KeyPair.CombineRecoveryKey(recoveryKey);
            var keyPair = KeyPair.CopyFromRecoveryKey(combinedKey);

            // Matches the ciphers declared by the V3 mock vault configuration
            return Security.CreateNew(
                keyPair,
                Constants.CipherId.XCHACHA20_POLY1305,
                Constants.CipherId.AES_SIV,
                Constants.CipherId.ENCODING_BASE4K);
        }

        private static string EncryptName(Security security, string plaintextName, ReadOnlySpan<byte> directoryId)
        {
            // Produces the name as the current implementation writes it, which is what the migration must arrive at
            return security.NameCrypt!.EncryptName(plaintextName, directoryId);
        }

        /// <summary>
        /// Rewrites a name produced by the current implementation into the form a V3 vault stored it in.
        /// </summary>
        private static string ToLegacyName(string currentEncoded)
        {
            var raw = SecombaBase4K.Decode(currentEncoded);
            raw.Should().NotBeNull();

            return Base4K.EncodeChainToString(raw!) + FileSystemNames.ENCRYPTED_FILE_EXTENSION;
        }

        private static byte[]? TryDecodeLegacy(string encoded)
        {
            try
            {
                return Base4K.DecodeChainToNewBuffer(encoded).ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<List<string>> GetItemNamesAsync(IFolder folder)
        {
            var names = new List<string>();
            await foreach (var item in folder.GetItemsAsync())
                names.Add(item.Name);

            return names;
        }
    }
}
