using System.Timers;
using ByteSizeLib;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.ViewModels.Dashboard;
using SecureFolderFS.Core.Tracking;

using Timer = System.Timers.Timer;

#nullable enable

namespace SecureFolderFS.Backend.Models
{
    public sealed class VaultIoSpeedReporterModel : IFileSystemStatsTracker
    {
        private readonly Timer _updateTimer;

        private long _readAmountBeforeFlush;

        private long _writeAmountBeforeFlush;

        private IThreadingService ThreadingService { get; } = Ioc.Default.GetRequiredService<IThreadingService>();

        public GraphWidgetControlViewModel? ReadGraphViewModel { get; init; }

        public GraphWidgetControlViewModel? WriteGraphViewModel { get; init; }

        public VaultIoSpeedReporterModel()
        {
            this._updateTimer = new Timer();
            this._updateTimer.Interval = 250d; // 1s
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

        private async void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;

            await ThreadingService.ExecuteOnUiThreadAsync(() =>
            {
                ReadGraphViewModel?.AddPoint(new()
                {
                    Date = now,
                    High = (long)ByteSize.FromBytes(_readAmountBeforeFlush).KiloBytes  * 8
                });
                WriteGraphViewModel?.AddPoint(new()
                {
                    Date = now,
                    High = (long)ByteSize.FromBytes(_writeAmountBeforeFlush).KiloBytes * 8
                });
            });

            _readAmountBeforeFlush = 0;
            _writeAmountBeforeFlush = 0;
        }

        public void Dispose()
        {
            _updateTimer.Stop();
            _updateTimer.Elapsed -= UpdateTimer_Elapsed;
        }
    }
}
