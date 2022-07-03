using System;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed class GraphPointViewModel
    {
        public long Value { get; }

        public DateTime Date { get; }

        public GraphPointViewModel(long value, DateTime date)
        {
            Value = value;
            Date = date;
        }
    }
}
