using System.Timers;
using ByteSizeLib;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Shared.Extensions;

using Timer = System.Timers.Timer;

namespace SecureFolderFS.Backend.Models
{
    public sealed class VaultIoSpeedReporterModel : BaseVaultIoSpeedReporterModel
    {
        private IThreadingService ThreadingService { get; } = Ioc.Default.GetRequiredService<IThreadingService>();

        private readonly Timer _updateTimer;

        private readonly List<long> _readRates;

        private readonly List<long> _writeRates;

        private int _updateTimerInterval;
        
        private long _readAmountBeforeFlush;

        private long _writeAmountBeforeFlush;

        private IProgress<double>? ReadGraphProgress { get; }

        private IProgress<double>? WriteGraphProgress { get; }

        public IGraphModel? ReadGraphModel { get; init; }

        public IGraphModel? WriteGraphModel { get; init; }

        public VaultIoSpeedReporterModel(IProgress<double>? readGraphProgress, IProgress<double>? writeGraphProgress)
        {
            this.ReadGraphProgress = readGraphProgress;
            this.WriteGraphProgress = writeGraphProgress;
            this._readRates = new() { 0 };
            this._writeRates = new() { 0 };
            this._updateTimer = new();
            this._updateTimer.Interval = 250d; // 0.25s
            this._updateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        public override void AddBytesRead(long amount)
        {
            _readAmountBeforeFlush += amount;
        }

        public override void AddBytesWritten(long amount)
        {
            _writeAmountBeforeFlush += amount;
        }

        public void Start()
        {
            _updateTimer.Start();
        }

        private void CalculateRates()
        {
            ReadGraphProgress?.Report(ByteSize.FromBytes(_readRates.Where(x => x != 0).AtLeast(() => 0).Average()).MegaBytes);
            WriteGraphProgress?.Report(ByteSize.FromBytes(_writeRates.Where(x => x != 0).AtLeast(() => 0).Average()).MegaBytes);
        }

        private async void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;

            await ThreadingService.ExecuteOnUiThreadAsync(() =>
            {
                ReadGraphModel?.AddPoint(new()
                {
                    Date = now,
                    High = Convert.ToInt64(Math.Round(ByteSize.FromBytes(_readAmountBeforeFlush).MegaBytes))
                });
                WriteGraphModel?.AddPoint(new()
                {
                    Date = now,
                    High = Convert.ToInt64(Math.Round(ByteSize.FromBytes(_writeAmountBeforeFlush).MegaBytes))
                });
            });

            _readRates.AddWithOverflow(_readAmountBeforeFlush, Constants.Graphs.MAX_GRAPH_RATES);
            _writeRates.AddWithOverflow(_writeAmountBeforeFlush, Constants.Graphs.MAX_GRAPH_RATES);

            _readAmountBeforeFlush = 0;
            _writeAmountBeforeFlush = 0;

            _updateTimerInterval++;
            if (_updateTimerInterval == 4)
            {
                CalculateRates();
                _updateTimerInterval = 0;
            }
        }

        public override void Dispose()
        {
            _updateTimer.Stop();
            _updateTimer.Elapsed -= UpdateTimer_Elapsed;
        }
    }
}
