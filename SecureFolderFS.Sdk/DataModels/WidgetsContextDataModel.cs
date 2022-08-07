using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed class WidgetsContextDataModel
    {
        public Dictionary<string, WidgetDataModel> WidgetDataModels { get; }

        [JsonConstructor]
        public WidgetsContextDataModel(Dictionary<string, WidgetDataModel>? widgetDataModels = null)
        {
            WidgetDataModels = widgetDataModels ?? new();
        }
    }
}
