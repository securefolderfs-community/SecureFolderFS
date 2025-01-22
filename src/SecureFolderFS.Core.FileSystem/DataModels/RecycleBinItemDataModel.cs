using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.FileSystem.DataModels
{
    [Serializable]
    public sealed class RecycleBinItemDataModel
    {
        /// <summary>
        /// Gets the original (relative) ciphertext path of the item before it was deleted.
        /// </summary>
        [JsonPropertyName("originalPath")]
        public required string? OriginalPath { get; init; }
        
        /// <summary>
        /// Gets the <see cref="DateTime"/> timestamp of the deletion.
        /// </summary>
        [JsonPropertyName("deletionTimestamp")]
        public required DateTime? DeletionTimestamp { get; init; }
    }
}
