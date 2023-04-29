using System;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed record WidgetDataModel(string WidgetId, string? WidgetsData = null)
    {
        public string? WidgetsData { get; set; } = WidgetsData;
    }
}
