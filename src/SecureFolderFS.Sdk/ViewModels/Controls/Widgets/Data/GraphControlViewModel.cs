﻿using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Data
{
    [Bindable(true)]
    public sealed partial class GraphControlViewModel : ObservableObject, IProgress<double>, IAsyncInitialize
    {
        [ObservableProperty] private bool _IsExtended;
        [ObservableProperty] private string? _GraphSubHeader = "0MB/s";
        [ObservableProperty] private ObservableCollection<double> _Data;

        public GraphControlViewModel()
        {
            _Data = new();
        }

        /// <inheritdoc/>
        public void Report(double value)
        {
            GraphSubHeader = $"{value.ToString("0.#")}MB/s";
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < Constants.Widgets.Graphs.MAX_GRAPH_POINTS; i++)
                Data.Add(0d);

            return Task.CompletedTask;
        }
    }
}
