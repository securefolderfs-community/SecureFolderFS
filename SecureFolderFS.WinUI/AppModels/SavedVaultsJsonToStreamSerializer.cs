using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SecureFolderFS.Sdk.AppModels;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <inheritdoc cref="JsonToStreamSerializer"/>
    internal sealed class SavedVaultsJsonToStreamSerializer : JsonToStreamSerializer
    {
        /// <inheritdoc/>
        public override async Task<TData?> DeserializeAsync<TData>(Stream serialized, CancellationToken cancellationToken = default) where TData : default
        {
            // A custom deserialization is needed, because the data returned is in JToken format
            var deserialized = await base.DeserializeAsync<TData>(serialized, cancellationToken);
            if (deserialized is Dictionary<string, object> dictionary)
            {
                var cleanedDictionary = new Dictionary<string, object>();
                foreach (var item in dictionary)
                {
                    if (item.Value is JArray jArray)
                        cleanedDictionary.Add(item.Key, jArray.Select(x => x.ToObject<string>()).ToList());
                    else
                        cleanedDictionary.Add(item.Key, item.Value);
                }

                return (TData?)(object)cleanedDictionary;
            }

            return deserialized;
        }
    }
}
