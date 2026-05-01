using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    public sealed record class VaultSharesDataModel
    {
        public List<VaultShareDataModel>? Shares { get; init; }
    }

    [Serializable]
    public sealed record class VaultShareDataModel
    {
        [JsonPropertyName("authOne")]
        public string? AuthenticationMethodId { get; init; }

        [JsonPropertyName("nonce")]
        public byte[]? Nonce { get; init; }

        [JsonPropertyName("c_complement")]
        public byte[]? WrappedComplementSecret { get; init; }

        [JsonPropertyName("tag")]
        public byte[]? Tag { get; init; }
    }
}