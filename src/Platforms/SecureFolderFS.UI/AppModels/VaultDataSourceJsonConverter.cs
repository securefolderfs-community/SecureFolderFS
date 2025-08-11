using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SecureFolderFS.Sdk.DataModels;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="JsonConverter{T}"/>
    internal sealed class VaultDataSourceJsonConverter : JsonConverter<VaultStorageSourceDataModel>
    {
        /// <inheritdoc/>
        public override VaultStorageSourceDataModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            var storageType = root.GetProperty("StorageType").GetString();
            return storageType switch
            {
                "LocalStorage" => JsonSerializer.Deserialize<LocalStorageSourceDataModel>(root.GetRawText(), options)!,
                "RemoteAccountStorage" => JsonSerializer.Deserialize<AccountSourceDataModel>(root.GetRawText(), options)!,
                _ => throw new JsonException($"Unknown vault storage type '{storageType}'.")
            };
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, VaultStorageSourceDataModel value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case LocalStorageSourceDataModel local:
                    JsonSerializer.Serialize(writer, local, options);
                    break;

                case AccountSourceDataModel account:
                    JsonSerializer.Serialize(writer, account, options);
                    break;

                default:
                    throw new JsonException($"Unknown {nameof(VaultStorageSourceDataModel)} subtype.");
            }
        }
    }
}
