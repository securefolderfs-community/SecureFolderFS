using ByteSizeLib;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed class GraphsWidgetViewModel : BaseWidgetViewModel
    {
        private readonly IReadWriteStatistics _readWriteStatistics;
        private readonly PeriodicTimer _periodicTimer;
        private readonly List<long> _readRates;
        private readonly List<long> _writeRates;
        private long _currentReadAmount;
        private long _currentWriteAmount;
        private int _updateTimeCount;

        public GraphControlViewModel ReadGraphViewModel { get; }

        public GraphControlViewModel WriteGraphViewModel { get; }

        public GraphsWidgetViewModel(IReadWriteStatistics readWriteStatistics, IWidgetModel widgetModel)
            : base(widgetModel)
        {
            ReadGraphViewModel = new();
            WriteGraphViewModel = new();
            _readWriteStatistics = readWriteStatistics;

            _periodicTimer = new(TimeSpan.FromMilliseconds(Constants.Graphs.GRAPH_UPDATE_INTERVAL_MS));
            _readRates = new() { 0 };
            _writeRates = new() { 0 };
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await ReadGraphViewModel.InitAsync(cancellationToken);
            await WriteGraphViewModel.InitAsync(cancellationToken);

            InitializeCallbacks();

            // We don't want to await it, since it's an async based timer
            _ = InitializeBlockingTimer(cancellationToken);
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
            _readWriteStatistics.BytesRead = new Progress<long>(x => _currentReadAmount += x);
            _readWriteStatistics.BytesWritten = new Progress<long>(x => _currentWriteAmount += x);
        }

        private void CalculateStatistics()
        {
            var read = Convert.ToInt64(ByteSize.FromBytes(_currentReadAmount).MegaBytes);
            var write = Convert.ToInt64(ByteSize.FromBytes(_currentWriteAmount).MegaBytes);

            var now = DateTime.Now;

            // Update graph for read
            var readPoint = ReadGraphViewModel.Data[0];
            readPoint.Value = read;
            readPoint.Date = now;
            ReadGraphViewModel.UpdateLastPoint();

            // Update graph for write
            var writePoint = WriteGraphViewModel.Data[0];
            writePoint.Date = now;
            writePoint.Value = write;
            WriteGraphViewModel.UpdateLastPoint();

            // Reset amounts
            _currentReadAmount = 0;
            _currentWriteAmount = 0;

            _updateTimeCount++;
            if (_updateTimeCount == Constants.Graphs.GRAPH_REFRESH_RATE)
            {
                _updateTimeCount = 0;

                var readRate = ByteSize.FromBytes(_readRates.Where(x => x != 0).Sum()).MegaBytes;
                var writeRate = ByteSize.FromBytes(_writeRates.Where(x => x != 0).Sum()).MegaBytes;

                ReadGraphViewModel.Report(readRate);
                WriteGraphViewModel.Report(writeRate);
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _readWriteStatistics.BytesRead = null;
            _readWriteStatistics.BytesWritten = null;
            _periodicTimer.Dispose();
        }
    }
}
