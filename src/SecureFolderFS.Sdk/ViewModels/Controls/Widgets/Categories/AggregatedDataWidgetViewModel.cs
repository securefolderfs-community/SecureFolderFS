using ByteSizeLib;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories
{
    [Bindable(true)]
    public sealed partial class AggregatedDataWidgetViewModel : BaseWidgetViewModel
    {
        private readonly IFileSystemStatistics _readWriteStatistics;
        private readonly PeriodicTimer _periodicTimer;
        private ByteSize _bytesRead;
        private ByteSize _bytesWritten;

        [ObservableProperty] private string? _TotalRead;
        [ObservableProperty] private string? _TotalWrite;
        
        public AggregatedDataWidgetViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IWidgetModel widgetModel)
            : base(widgetModel)
        {
            _readWriteStatistics = unlockedVaultViewModel.StorageRoot.ReadWriteStatistics;
            _periodicTimer = new(TimeSpan.FromMilliseconds(Constants.Graphs.GRAPH_UPDATE_INTERVAL_MS));
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            _bytesRead = new();
            _bytesWritten = new();
            _readWriteStatistics.BytesRead = new Progress<long>(x => _bytesRead.AddBytes(x));
            _readWriteStatistics.BytesWritten = new Progress<long>(x => _bytesWritten.AddBytes(x));
            
            // We don't want to await it, since it's an async based timer
            _ = InitializeBlockingTimer(cancellationToken);
            
            return Task.CompletedTask;
        }

        private async Task InitializeBlockingTimer(CancellationToken cancellationToken)
        {
            while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                TotalRead = _bytesRead.ToString();
                TotalWrite = _bytesWritten.ToString();
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
