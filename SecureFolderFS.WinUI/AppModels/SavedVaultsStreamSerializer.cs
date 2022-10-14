using SecureFolderFS.Sdk.AppModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <inheritdoc cref="StreamSerializer"/>
    internal sealed class SavedVaultsStreamSerializer : StreamSerializer
    {
        /// <inheritdoc/>
        public override async Task<object?> DeserializeAsync(Stream serialized, Type dataType, CancellationToken cancellationToken = default)
        {
            // A custom deserialization is needed, because the data returned is in JToken format
            var deserialized = await base.DeserializeAsync(serialized, dataType, cancellationToken);
            if (deserialized is Dictionary<string, object> dictionary)
            {
                var cleanedDictionary = new Dictionary<string, object>();
                foreach (var item in dictionary)
                {
                    if (item.Value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                    {
                        cleanedDictionary.Add(item.Key, jsonElement.EnumerateArray().Select(x => x.GetString()).ToList());
                    }
                    else
                        cleanedDictionary.Add(item.Key, item.Value);
                }

                return cleanedDictionary;
            }

            return deserialized;
        }
    }
}
