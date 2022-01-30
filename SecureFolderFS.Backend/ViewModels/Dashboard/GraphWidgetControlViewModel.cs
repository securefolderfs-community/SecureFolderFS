using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Backend.Models;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dashboard
{
    public sealed class GraphWidgetControlViewModel : ObservableObject
    {
        public ObservableCollection<GraphPointModel> Data { get; }

        private string? _GraphSubheader;
        public string? GraphSubheader
        {
            get => _GraphSubheader;
            set => SetProperty(ref _GraphSubheader, value);
        }

        private bool _GraphLoaded;
        public bool GraphLoaded
        {
            get => _GraphLoaded;
            set => SetProperty(ref _GraphLoaded, value);
        }

        public void AddPoint(GraphPointModel graphPointModel)
        {
            if (!GraphLoaded)
            {
                return;
            }

            if (Data.Count == Constants.MAX_GRAPH_POINTS)
            {
                Data.RemoveAt(0);
            }

            Data.Add(graphPointModel);
        }

        public GraphWidgetControlViewModel()
        {
            Data = new();
        }
    }
}
