using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed record class WidgetsCollectionDataModel(IDictionary<string, WidgetDataModel> WidgetDataModels)
    {
        public IDictionary<string, WidgetDataModel> WidgetDataModels { get; } = WidgetDataModels;
    }
}
