using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed class GraphControlViewModel : ObservableObject, IGraphModel, IProgress<double>
    {
        public ObservableCollection<GraphPointModel> Data { get; }

        public bool IsExtended { get; set; }

        private string? _GraphSubheader = "0mb/s";
        public string? GraphSubheader
        {
            get => _GraphSubheader;
            set => SetProperty(ref _GraphSubheader, value);
        }

        private bool _IsReady;
        public bool IsReady
        {
            get => _IsReady;
            set => SetProperty(ref _IsReady, value);
        }

        public GraphControlViewModel()
        {
            Data = new();
        }

        public void AddPoint(GraphPointModel graphPointModel)
        {
            if (!IsReady)
            {
                return;
            }

            try
            {
                if (Data.Count == Constants.Graphs.MAX_GRAPH_POINTS)
                {
                    Data.RemoveAt(0);
                }

                Data.Add(graphPointModel);
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        public void Report(double value)
        {
            GraphSubheader = $"{value.ToString("0.#")}mb/s";
        }
    }
}
