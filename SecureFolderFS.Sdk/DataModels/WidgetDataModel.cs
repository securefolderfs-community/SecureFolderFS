using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed class WidgetDataModel
    {
        public Dictionary<string, object>? WidgetData { get; set; }
    }
}
