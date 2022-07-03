using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed class GraphControlViewModel : ObservableObject
    {
        public ObservableCollection<GraphPointViewModel> Data { get; }

        private string? _GraphSubheader = "0mb/s";
        public string? GraphSubheader
        {
            get => _GraphSubheader;
            set => SetProperty(ref _GraphSubheader, value);
        }

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

        public void Report(double value)
        {
            GraphSubheader = $"{value.ToString("0.#")}mb/s";
        }
    }
}
