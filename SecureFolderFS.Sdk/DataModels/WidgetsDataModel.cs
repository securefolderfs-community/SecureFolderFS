using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed class WidgetsDataModel
    {
        public Dictionary<string, WidgetDataModel> WidgetDataModels { get; }

        [JsonConstructor]
        public WidgetsDataModel(Dictionary<string, WidgetDataModel>? widgetDataModels = null)
        {
            WidgetDataModels = widgetDataModels ?? new();
        }
    }
}
