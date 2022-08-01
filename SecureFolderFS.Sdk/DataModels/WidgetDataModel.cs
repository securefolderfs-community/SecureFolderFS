using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed class WidgetDataModel
    {
        public Dictionary<string, object?>? WidgetData { get; set; }
    }
}
