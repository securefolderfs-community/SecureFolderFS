using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed record class WidgetDataModel(IDictionary<string, string?> WidgetsData)
    {
        public IDictionary<string, string?> WidgetsData { get; } = WidgetsData;
    }
}
