using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class VaultLiveStatisticsModel : IVaultStatisticsModel, IAsyncInitialize
    {
        private readonly PeriodicTimer _periodicTimer;
        private readonly List<long> _readRates;
        private readonly List<long> _writeRates;
        private long _currentReadAmount;
        private long _currentWriteAmount;

        public VaultLiveStatisticsModel()
        {
            _periodicTimer = new(TimeSpan.FromMilliseconds(100)); // 0.1s
            _readRates = new() { 0 };
            _writeRates = new() { 0 };
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                _readRates.AddWithMaxCapacity(_currentReadAmount, Constants.Graphs.MAX_GRAPH_RATES);
                _writeRates.AddWithMaxCapacity(_currentWriteAmount, Constants.Graphs.MAX_GRAPH_RATES);

                CalculateStatistics();
            }
        }

        private void CalculateStatistics()
        {
            var read = Convert.ToInt64(ByteSize.FromBytes(_currentReadAmount).MegaBytes);
            var write = Convert.ToInt64(ByteSize.FromBytes(_currentWriteAmount).MegaBytes);

            var readRate = ByteSize.FromBytes(_readRates.Where(x => x != 0).AtLeast(0).Average()).MegaBytes;
            var writeRate = ByteSize.FromBytes(_writeRates.Where(x => x != 0).AtLeast(0).Average()).MegaBytes;
        }

        /// <inheritdoc/>
        public void NotifyBytesRead(long amount)
        {
            _currentReadAmount += amount;
        }

        /// <inheritdoc/>
        public void NotifyBytesWritten(long amount)
        {
            _currentWriteAmount += amount;
        }
    }
}
