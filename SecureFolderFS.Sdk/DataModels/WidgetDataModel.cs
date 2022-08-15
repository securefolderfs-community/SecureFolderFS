using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed class WidgetDataModel
    {
        public Dictionary<string, object?> WidgetData { get; }

        [JsonConstructor]
        public WidgetDataModel(Dictionary<string, object?>? widgetData = null)
        {
            WidgetData = widgetData ?? new();
        }
    }
}
