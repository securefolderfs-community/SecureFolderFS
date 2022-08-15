using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed class GraphsWidgetViewModel : BaseWidgetViewModel
    {
        private readonly IVaultStatisticsModel _vaultStatisticsModel;
        private readonly PeriodicTimer _periodicTimer;
        private readonly List<long> _readRates;
        private readonly List<long> _writeRates;
        private long _currentReadAmount;
        private long _currentWriteAmount;
        private int _updateTimeCount;

        public GraphControlViewModel ReadGraphViewModel { get; }

        public GraphControlViewModel WriteGraphViewModel { get; }

        public GraphsWidgetViewModel(IVaultStatisticsModel vaultStatisticsModel, IWidgetModel widgetModel)
            : base(widgetModel)
        {
            ReadGraphViewModel = new();
            WriteGraphViewModel = new();
            _vaultStatisticsModel = vaultStatisticsModel;

            _periodicTimer = new(TimeSpan.FromMilliseconds(Constants.Graphs.GRAPH_UPDATE_INTERVAL_MS));
            _readRates = new() { 0 };
            _writeRates = new() { 0 };
        }
        
        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            InitializeCallbacks();

            // We don't want to await it, since it's an async based timer
            _ = InitializeBlockingTimer(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task InitializeBlockingTimer(CancellationToken cancellationToken)
        {
            while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                _readRates.AddWithMaxCapacity(_currentReadAmount, Constants.Graphs.GRAPH_REFRESH_RATE);
                _writeRates.AddWithMaxCapacity(_currentWriteAmount, Constants.Graphs.GRAPH_REFRESH_RATE);

                CalculateStatistics();
            }
        }

        private void InitializeCallbacks()
        {
            _vaultStatisticsModel.NotifyForBytesRead(x => _currentReadAmount += x);
            _vaultStatisticsModel.NotifyForBytesWritten(x => _currentWriteAmount += x);
        }

        private void CalculateStatistics()
        {
            var read = Convert.ToInt64(ByteSize.FromBytes(_currentReadAmount).MegaBytes);
            var write = Convert.ToInt64(ByteSize.FromBytes(_currentWriteAmount).MegaBytes);

            var now = DateTime.Now;
            ReadGraphViewModel.AddPoint(new(read, now));
            WriteGraphViewModel.AddPoint(new(write, now));

            _currentReadAmount = 0;
            _currentWriteAmount = 0;

            _updateTimeCount++;
            if (_updateTimeCount == Constants.Graphs.GRAPH_REFRESH_RATE)
            {
                _updateTimeCount = 0;

                var readRate = ByteSize.FromBytes(_readRates.Where(x => x != 0).IfEmptyThenAppend(0).Sum()).MegaBytes;
                var writeRate = ByteSize.FromBytes(_writeRates.Where(x => x != 0).IfEmptyThenAppend(0).Sum()).MegaBytes;

                ReadGraphViewModel.Report(readRate);
                WriteGraphViewModel.Report(writeRate);
            }
        }
    }
}
