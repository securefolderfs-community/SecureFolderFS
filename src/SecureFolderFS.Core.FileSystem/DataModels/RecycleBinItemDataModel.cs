using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.FileSystem.DataModels
{
    [Serializable]
    public sealed record class RecycleBinItemDataModel
    {
        /// <summary>
        /// Gets the original ciphertext name of the item before it was deleted.
        /// </summary>
        [JsonPropertyName("originalName")]
        public required string? OriginalName { get; init; }

        /// <summary>
        /// Gets the original (relative) ciphertext path of the folder where the item resided before it was deleted.
        /// </summary>
        [JsonPropertyName("parentPath")]
        public required string? ParentPath { get; init; }

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
    }
}
