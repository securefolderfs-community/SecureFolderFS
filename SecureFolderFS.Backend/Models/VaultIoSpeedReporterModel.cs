using System.Timers;
using ByteSizeLib;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.ViewModels.Dashboard;
using SecureFolderFS.Core.Tracking;
using SecureFolderFS.Shared.Extensions;

using Timer = System.Timers.Timer;

namespace SecureFolderFS.Backend.Models
{
    public sealed class VaultIoSpeedReporterModel : IFileSystemStatsTracker
    {
        private readonly Timer _updateTimer;

        private readonly List<long> _readRates;

        private readonly List<long> _writeRates;

        private long _readAmountBeforeFlush;
        
        private long _writeAmountBeforeFlush;

        private int _updateTimerInterval;

        private IThreadingService ThreadingService { get; } = Ioc.Default.GetRequiredService<IThreadingService>();

        public GraphWidgetControlViewModel? ReadGraphViewModel { get; init; }

        public GraphWidgetControlViewModel? WriteGraphViewModel { get; init; }

        public VaultIoSpeedReporterModel()
        {
            this._readRates = new() { 0 };
            this._writeRates = new() { 0 };
            this._updateTimer = new();
            this._updateTimer.Interval = 250d; // 0.25s
            this._updateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        public void AddBytesRead(long amount)
        {
            _readAmountBeforeFlush += amount;
        }

        public void AddBytesWritten(long amount)
        {
            _writeAmountBeforeFlush += amount;
        }

        #region Unused

        public void AddBytesEncrypted(long amount)
        {
        }

        public void AddBytesDecrypted(long amount)
        {
        }

        public void AddChunkCacheMiss()
        {
        }

        public void AddChunkCacheHit()
        {
        }

        public void AddChunkAccess()
        {
        }

        public void AddDirectoryIdCacheMiss()
        {
        }

        public void AddDirectoryIdCacheHit()
        {
        }

        public void AddDirectoryIdAccess()
        {
        }

        public void AddFileNameCacheMiss()
        {
        }

        public void AddFileNameCacheHit()
        {
        }

        public void AddFileNameAccess()
        {
        }

        #endregion Unused

        public void Start()
        {
            _updateTimer.Start();
        }

        private async Task CalculateRates()
        {
            if (ReadGraphViewModel == null || WriteGraphViewModel == null)
            {
                return;
            }

            await ThreadingService.ExecuteOnUiThreadAsync(() =>
            {
                ReadGraphViewModel.GraphSubheader = $"{ByteSize.FromBytes(_readRates.Where(x => x != 0).AtLeast(0).Average()).MegaBytes.ToString("0.#")}mb/s";
                WriteGraphViewModel.GraphSubheader = $"{ByteSize.FromBytes(_writeRates.Where(x => x != 0).AtLeast(0).Average()).MegaBytes.ToString("0.#")}mb/s";
            });
        }

        private async void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;

            await ThreadingService.ExecuteOnUiThreadAsync(() =>
            {
                ReadGraphViewModel?.AddPoint(new()
                {
                    Date = now,
                    High = (long)ByteSize.FromBytes(_readAmountBeforeFlush).MegaBytes
                });
                WriteGraphViewModel?.AddPoint(new()
                {
                    Date = now,
                    High = (long)ByteSize.FromBytes(_writeAmountBeforeFlush).MegaBytes
                });
            });

            _readRates.AddWithOverflow(_readAmountBeforeFlush, Constants.MAX_GRAPH_RATES);
            _writeRates.AddWithOverflow(_writeAmountBeforeFlush, Constants.MAX_GRAPH_RATES);

            _readAmountBeforeFlush = 0;
            _writeAmountBeforeFlush = 0;

            _updateTimerInterval++;
            if (_updateTimerInterval == 4)
            {
                await CalculateRates();

                _updateTimerInterval = 0;
            }
        }

        public void Dispose()
        {
            _updateTimer.Stop();
            _updateTimer.Elapsed -= UpdateTimer_Elapsed;
        }
    }
}
