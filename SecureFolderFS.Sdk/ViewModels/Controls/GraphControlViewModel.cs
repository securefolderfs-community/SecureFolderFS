using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.Utils;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed partial class GraphControlViewModel : ObservableObject, IProgress<double>, IAsyncInitialize
    {
        public ObservableCollection<GraphPointViewModel> Data { get; }

        [ObservableProperty]
        private string? _GraphSubheader = "0mb/s";

        public GraphControlViewModel()
        {
            Data = new();
        }

        public void UpdateLastPoint()
        {
            Data.Move(0, Data.Count - 1);
        }

        /// <inheritdoc/>
        public void Report(double value)
        {
            GraphSubheader = $"{value.ToString("0.#")}mb/s";
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < Constants.Graphs.MAX_GRAPH_POINTS; i++)
            {
                Data.Add(new());
            }

            return Task.CompletedTask;
        }
    }
}
