using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed class WidgetDataModel
    {
        public Dictionary<string, string?> WidgetData { get; }

        [JsonConstructor]
        public WidgetDataModel(Dictionary<string, string?>? widgetData = null)
        {
            WidgetData = widgetData ?? new();
        }
    }
}
