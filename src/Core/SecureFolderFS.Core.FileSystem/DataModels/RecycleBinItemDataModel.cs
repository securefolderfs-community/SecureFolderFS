using System;
using System.IO;
using System.Text.Json.Serialization;
using SecureFolderFS.Core.Cryptography;

namespace SecureFolderFS.Core.FileSystem.DataModels
{
    [Serializable]
    public sealed record class RecycleBinItemDataModel
    {
        /// <summary>
        /// Gets the original ciphertext name of the item before it was deleted.
        /// </summary>
        [JsonPropertyName("c_originalName")]
        public required string? Name { get; init; }

        /// <summary>
        /// Gets the fully encrypted ciphertext path of the folder where the item resided before it was deleted.
        /// </summary>
        [JsonPropertyName("c_parentId")]
        public required string? ParentId { get; init; }

        /// <summary>
        /// Gets the Directory ID of the directory where this item originally belonged to.
        /// </summary>
        [JsonPropertyName("directoryId")]
        public required byte[]? DirectoryId { get; init; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> timestamp of the deletion.
        /// </summary>
        [JsonPropertyName("deletionTimestamp")]
        public required DateTime? DeletionTimestamp { get; init; }

        /// <summary>
        /// Gets the size in bytes of the item. The value might be less than zero indicating that the size was not calculated.
        /// </summary>
        [JsonPropertyName("size")]
        public required long? Size { get; init; }

        // TODO: Add MAC key signing for tamper proofing

        public string? DecryptName(Security security)
        {
            if (security.NameCrypt is null)
                return Name;

            return security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(Name), DirectoryId);
        }

        public string? DecryptParentId(Security security)
        {
            if (security.NameCrypt is null)
                return ParentId;

            return security.NameCrypt.DecryptName(ParentId, DirectoryId);
        }

        public static string Encrypt(string plaintext, Security security, ReadOnlySpan<byte> directoryId)
        {
            if (security.NameCrypt is null)
                return plaintext;

            return security.NameCrypt.EncryptName(plaintext, directoryId);
        }
    }
}
