using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed partial class GraphControlViewModel : ObservableObject, IProgress<double>
    {
        public ObservableCollection<GraphPointViewModel> Data { get; }

        [ObservableProperty]
        private string? _GraphSubheader = "0mb/s";

        public GraphControlViewModel()
        {
            Data = new();
        }

        public void AddPoint(GraphPointViewModel graphPoint)
        {
            if (Data.Count >= Constants.Graphs.MAX_GRAPH_POINTS)
                Data.RemoveAt(0);

            Data.Add(graphPoint);
        }

        /// <inheritdoc/>
        public void Report(double value)
        {
            GraphSubheader = $"{value.ToString("0.#")}mb/s";
        }
    }
}
