using ByteSizeLib;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Data
{
    [Bindable(true)]
    public sealed partial class GraphsWidgetViewModel : BaseWidgetViewModel
    {
        private readonly IFileSystemStatistics _fileSystemStatistics;
        private readonly PeriodicTimer _periodicTimer;
        private readonly List<long> _readRates;
        private readonly List<long> _writeRates;
        private long _currentReadAmount;
        private long _currentWriteAmount;
        private int _updateTimeCount;

        [ObservableProperty] private GraphControlViewModel _ReadGraphViewModel;
        [ObservableProperty] private GraphControlViewModel _WriteGraphViewModel;
        [ObservableProperty] private bool _IsActive;

        public GraphsWidgetViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IWidgetModel widgetModel)
            : base(widgetModel)
        {
            IsActive = true;
            ReadGraphViewModel = new();
            WriteGraphViewModel = new();
            _fileSystemStatistics = unlockedVaultViewModel.StorageRoot.Options.FileSystemStatistics;

            _periodicTimer = new(TimeSpan.FromMilliseconds(Constants.Widgets.Graphs.GRAPH_UPDATE_INTERVAL_MS));
            _readRates = [ 0 ];
            _writeRates = [ 0 ];
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await ReadGraphViewModel.InitAsync(cancellationToken);
            await WriteGraphViewModel.InitAsync(cancellationToken);

            _fileSystemStatistics.BytesRead = new Progress<long>(x => _currentReadAmount += x);
            _fileSystemStatistics.BytesWritten = new Progress<long>(x => _currentWriteAmount += x);
            
            // We don't want to await it, since it's an async based timer
            _ = InitializeBlockingTimer(cancellationToken);
        }

        private async Task InitializeBlockingTimer(CancellationToken cancellationToken)
        {
            while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                if (!IsActive)
                    continue;

                _readRates.AddWithMaxCapacity(_currentReadAmount, Constants.Widgets.Graphs.GRAPH_REFRESH_RATE);
                _writeRates.AddWithMaxCapacity(_currentWriteAmount, Constants.Widgets.Graphs.GRAPH_REFRESH_RATE);

                CalculateStatistics();
            }
        }
        
        private void CalculateStatistics()
        {
            var read = Convert.ToInt64(ByteSize.FromBytes(_currentReadAmount).MegaBytes);
            var write = Convert.ToInt64(ByteSize.FromBytes(_currentWriteAmount).MegaBytes);

            // Update graph for read
            ReadGraphViewModel.Data.RemoveAt(0);
            ReadGraphViewModel.Data.Add(read);

            // Update graph for write
            WriteGraphViewModel.Data.RemoveAt(0);
            WriteGraphViewModel.Data.Add(write);

            // Reset amounts
            _currentReadAmount = 0;
            _currentWriteAmount = 0;

            _updateTimeCount++;
            if (_updateTimeCount == Constants.Widgets.Graphs.GRAPH_REFRESH_RATE)
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
            _fileSystemStatistics.BytesRead = null;
            _fileSystemStatistics.BytesWritten = null;
            _periodicTimer.Dispose();
        }
    }
}
