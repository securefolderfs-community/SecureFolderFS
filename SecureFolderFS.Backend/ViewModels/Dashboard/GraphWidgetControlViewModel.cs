using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Backend.Models;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dashboard
{
    public sealed class GraphWidgetControlViewModel : ObservableObject
    {
        public ObservableCollection<GraphPointModel> Data { get; }

        public IDisposable? GraphDisposable { get; set; }

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

        public GraphWidgetControlViewModel()
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
                if (Data.Count == Constants.MAX_GRAPH_POINTS)
                {
                    Data.RemoveAt(0);
                }

                Data.Add(graphPointModel);
            }
            catch (Exception) { }
        }
    }
}
