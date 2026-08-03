using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
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

        /// <summary>
        /// Gets the HMAC-SHA256 tag binding this model's fields to the payload it describes.
        /// </summary>
        [JsonPropertyName("hmacsha256mac")]
        public byte[]? PayloadMac { get; set; }

        public string? DecryptName(Security security)
        {
            var plaintextName = security.NameCrypt is null
                ? Name
                : security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(Name), DirectoryId);

            // The name is later joined onto a folder path to reattach or restore the payload, and a name
            // that is not a single path component would land that payload wherever the attacker chose (e.g., autostart dir).
            // AES-SIV already yields separator-free tokens, but CipherId.NONE returns the name verbatim.
            // Checked independently of the MAC, because the vault's own code writes these names and a correctly signed one must not escape either
            if (plaintextName is null || !IsSingleNameComponent(plaintextName))
                return null;

            return plaintextName;
        }

        /// <summary>
        /// Returns a copy of this model carrying a <see cref="PayloadMac"/> over its fields and <paramref name="itemName"/>.
        /// </summary>
        /// <param name="itemName">The name of the payload in the recycle bin that this model describes.</param>
        /// <param name="security">The <see cref="Security"/> instance holding the vault's MAC key.</param>
        public RecycleBinItemDataModel WithMac(string itemName, Security security)
        {
            return this with { PayloadMac = ComputeMac(this, itemName, security) };
        }

        /// <summary>
        /// Determines whether <see cref="PayloadMac"/> authenticates this model against <paramref name="itemName"/>.
        /// </summary>
        /// <param name="itemName">The name of the payload in the recycle bin that this model describes.</param>
        /// <param name="security">The <see cref="Security"/> instance holding the vault's MAC key.</param>
        /// <remarks>
        /// Every gate on the reattachment and restore paths - the Directory ID lineage match, the deletion
        /// recency window, and the original parent path - reads its evidence from this model. Without the
        /// tag those gates are authored by whoever can write into the vault's ciphertext directory.
        /// </remarks>
        public bool VerifyMac(string itemName, Security security)
        {
            if (PayloadMac is not { Length: HMACSHA256.HashSizeInBytes })
                return false;

            return CryptographicOperations.FixedTimeEquals(ComputeMac(this, itemName, security), PayloadMac);
        }

        // [SkipLocalsInit] - deliberately not used here.
        private static byte[] ComputeMac(RecycleBinItemDataModel dataModel, string itemName, Security security)
        {
            var mac = new byte[HMACSHA256.HashSizeInBytes];
            security.KeyPair.MacKey.UseKey(macKey =>
            {
                using var hmac = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, macKey);

                // Every field is length-prefixed. Concatenating them raw would let bytes be shifted
                // across the boundary between two adjacent attacker-influenced fields (Name + ParentId) to yield a different model with an identical MAC input.
                // The payload's own name is bound too, so a valid configuration cannot be paired with a different payload file
                AppendField(hmac, Encoding.UTF8.GetBytes(itemName), true);
                AppendField(hmac, dataModel.Name is null ? default : Encoding.UTF8.GetBytes(dataModel.Name), dataModel.Name is not null);
                AppendField(hmac, dataModel.ParentId is null ? default : Encoding.UTF8.GetBytes(dataModel.ParentId), dataModel.ParentId is not null);
                AppendField(hmac, dataModel.DirectoryId, dataModel.DirectoryId is not null);

                // Ticks and Kind are bound exactly as serialized, so no timezone conversion sits
                // between signing and verification
                Span<byte> timestamp = stackalloc byte[sizeof(long) + 1];
                if (dataModel.DeletionTimestamp is { } deletionTimestamp)
                {
                    BinaryPrimitives.WriteInt64LittleEndian(timestamp, deletionTimestamp.Ticks);
                    timestamp[sizeof(long)] = (byte)deletionTimestamp.Kind;
                }

                AppendField(hmac, timestamp, dataModel.DeletionTimestamp is not null);

                Span<byte> size = stackalloc byte[sizeof(long)];
                if (dataModel.Size is { } sizeValue)
                    BinaryPrimitives.WriteInt64LittleEndian(size, sizeValue);

                AppendField(hmac, size, dataModel.Size is not null);

                _ = hmac.GetHashAndReset(mac);
            });

            return mac;

            [SkipLocalsInit]
            static void AppendField(IncrementalHash hmac, ReadOnlySpan<byte> value, bool isPresent)
            {
                // The presence flag keeps an absent field distinct from a present-but-empty one
                Span<byte> header = stackalloc byte[sizeof(int) + 1];
                header[0] = isPresent ? (byte)1 : (byte)0;
                BinaryPrimitives.WriteInt32LittleEndian(header[1..], isPresent ? value.Length : 0);

                hmac.AppendData(header);
                if (isPresent)
                    hmac.AppendData(value);
            }
        }

        /// <summary>
        /// Determines whether <paramref name="name"/> is a single path component that cannot escape its parent.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <remarks>
        /// Both separators are checked regardless of the running platform.
        /// </remarks>
        private static bool IsSingleNameComponent(string name)
        {
            return !string.IsNullOrEmpty(name)
                   && name is not ("." or "..")
                   && name.IndexOf('/') < 0
                   && name.IndexOf('\\') < 0
                   && !Path.IsPathRooted(name);
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
