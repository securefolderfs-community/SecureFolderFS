using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed partial class GraphControlViewModel : ObservableObject, IProgress<double>, IAsyncInitialize
    {
        [ObservableProperty] private ObservableCollection<GraphPoint> _Data;
        [ObservableProperty] private bool _IsExtended;
        [ObservableProperty] private string? _GraphSubheader = "0mb/s";

        public GraphControlViewModel()
        {
            _Data = new();
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
                Data.Add(new(0, DateTime.Now));
            }

            return Task.CompletedTask;
        }
    }

    public sealed class GraphPoint
    {
        public long Value { get; set; }

        public DateTime Date { get; set; }

        public GraphPoint(long value, DateTime date)
        {
            Value = value;
            Date = date;
        }
    }
}
