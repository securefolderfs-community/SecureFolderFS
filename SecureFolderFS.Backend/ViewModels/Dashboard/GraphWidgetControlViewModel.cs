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

        public GraphWidgetControlViewModel()
        {
            Data = new();
        }
    }
}
