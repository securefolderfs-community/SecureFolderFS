using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

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

        public GraphPointViewModel RentGraphPoint()
        {
            GraphPointViewModel graphPoint;
            if (Data.Count < Constants.Graphs.MAX_GRAPH_POINTS)
            {
                graphPoint = new();
            }
            else
            {
                graphPoint = Data[0];
                Data.RemoveAt(0);
            }

            return graphPoint;
        }

        public void ReturnGraphPoint(GraphPointViewModel graphPoint)
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
