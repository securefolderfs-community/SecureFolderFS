using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Data
{
    [Bindable(true)]
    public sealed partial class AggregatedDataWidgetViewModel : BaseWidgetViewModel
    {
        private readonly IFileSystemStatistics _fileSystemStatistics;
        private readonly PeriodicTimer _periodicTimer;
        private IDisposable? _bytesReadSubscription;
        private IDisposable? _bytesWrittenSubscription;
        private ulong _pendingBytesRead;
        private ulong _pendingBytesWritten;
        private ByteSize _bytesRead;
        private ByteSize _bytesWritten;
        private int _updateTicks;

        [ObservableProperty] private string? _TotalRead;
        [ObservableProperty] private string? _TotalWrite;
        [ObservableProperty] private bool _IsReading;
        [ObservableProperty] private bool _IsWriting;

        public AggregatedDataWidgetViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IWidgetModel widgetModel)
            : base(widgetModel)
        {
            _fileSystemStatistics = unlockedVaultViewModel.StorageRoot.Options.FileSystemStatistics;
            _periodicTimer = new(TimeSpan.FromMilliseconds(Constants.Widgets.AggregatedData.UPDATE_INTERVAL_MS));
            Title = "AggregatedDataWidget".ToLocalized();
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            _bytesRead = new();
            _bytesWritten = new();
            TotalRead = "0B";
            TotalWrite = "0B";

            // Subscribe to statistics if it supports subscription
            if (_fileSystemStatistics is IFileSystemStatisticsSubscriber subscriber)
            {
                _bytesReadSubscription = subscriber.SubscribeToBytesRead(new Progress<long>(x =>
                {
                    if (x <= 0L)
                        return;

                    IsReading = true;
                    _pendingBytesRead += (ulong)x;
                }));

                _bytesWrittenSubscription = subscriber.SubscribeToBytesWritten(new Progress<long>(x =>
                {
                    if (x <= 0L)
                        return;

                    IsWriting = true;
                    _pendingBytesWritten += (ulong)x;
                }));
            }

            // We don't want to await it, since it's an async based timer
            _ = InitializeBlockingTimer(cancellationToken);

            return Task.CompletedTask;
        }

        private async Task InitializeBlockingTimer(CancellationToken cancellationToken)
        {
            while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                if (_pendingBytesRead > 0UL)
                {
                    _bytesRead = _bytesRead.AddBytes(_pendingBytesRead);
                    TotalRead = _bytesRead.ToString().Replace(" ", string.Empty);
                    _pendingBytesRead = 0UL;
                }

                if (_pendingBytesWritten > 0UL)
                {
                    _bytesWritten = _bytesWritten.AddBytes(_pendingBytesWritten);
                    TotalWrite = _bytesWritten.ToString().Replace(" ", string.Empty);
                    _pendingBytesWritten = 0UL;
                }

                _updateTicks++;
                if (_updateTicks >= Constants.Widgets.AggregatedData.REFRESH_RATE)
                {
                    _updateTicks = 0;
                    IsReading = false;
                    IsWriting = false;
                }
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _bytesReadSubscription?.Dispose();
            _bytesWrittenSubscription?.Dispose();
            _periodicTimer.Dispose();
        }
    }
}
