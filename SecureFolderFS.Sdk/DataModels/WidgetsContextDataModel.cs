using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed class WidgetsContextDataModel
    {
        public Dictionary<string, WidgetDataModel>? WidgetDataModels { get; set; }
    }
}
