using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.FileSystem.DataModels
{
    [Serializable]
    public sealed record class RecycleBinDataModel
    {
        /// <summary>
        /// Gets or sets the accumulated total size of the contents currently present in the recycle bin, measured in bytes.
        /// </summary>
        [JsonPropertyName("occupiedSize")]
        [DefaultValue(0)]
        public long OccupiedSize { get; set; }
    }
}
