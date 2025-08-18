using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Scanners;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IHealthModel"/>
    [Inject<IVaultService>]
    public sealed partial class HealthModel : IHealthModel, IProgress<IResult>
    {
        private readonly IFolderScanner _folderScanner;
        private readonly List<IChildFile> _scannedFiles;
        private readonly List<IChildFolder> _scannedFolders;
        private readonly ProgressModel<TotalProgress> _progress;
        private readonly IAsyncValidator<(IFolder, IProgress<IResult>?), IResult>? _structureValidator;
        private int _updateCount;
        private int _updateInterval;
        private volatile int _totalFilesScanned;
        private volatile int _totalFoldersScanned;

        /// <inheritdoc/>
        public event EventHandler<HealthIssueEventArgs>? IssueFound;

        public HealthModel(IFolderScanner folderScanner, ProgressModel<TotalProgress> progress, IAsyncValidator<(IFolder, IProgress<IResult>?), IResult>? structureValidator)
        {
            ServiceProvider = DI.Default;
            _scannedFiles = new();
            _scannedFolders = new();
            _folderScanner = folderScanner;
            _progress = progress;
            _structureValidator = structureValidator;
        }

        /// <inheritdoc/>
        public async Task ScanAsync(bool includeFileContents, CancellationToken cancellationToken = default)
        {
            await Task.Run(async () =>
            {
                _progress.PercentageProgress?.Report(0d);

                _totalFilesScanned = 0;
                _totalFoldersScanned = 0;
                _scannedFiles.Clear();
                _scannedFolders.Clear();

                // Start collecting items
                _progress.CallbackProgress?.Report(new());
                await Task.Delay(750, cancellationToken);

                // Do not forget to add the RootFolder
                AddScannedItem(_folderScanner.RootFolder);

                // Continue enumeration as usual
                await foreach (var item in _folderScanner.ScanFolderAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (VaultService.IsNameReserved(item.Name))
                        continue;

                    AddScannedItem(item);
                }

                if (Constants.Widgets.Health.ARE_UPDATES_OPTIMIZED)
                {
                    var totalUpdatesOptimized = (int)((_scannedFiles.Count + _scannedFolders.Count) * Constants.Widgets.Health.INTERVAL_MULTIPLIER);
                    _updateInterval = (_scannedFiles.Count + _scannedFolders.Count) / Math.Max(100, totalUpdatesOptimized);
                }

                // Report initial progress
                ReportProgress(_progress);
                await Task.Delay(750, cancellationToken);

                // Begin scanning
                await ScanStructureAsync(cancellationToken).ConfigureAwait(false);

                // Report final progress
                ReportProgress(_progress);
                await Task.Delay(1500, cancellationToken);
            }, cancellationToken).ConfigureAwait(false);
        }

        private void AddScannedItem(IStorable storable)
        {
            switch (storable)
            {
                case IChildFile file:
                    _scannedFiles.Add(file);
                    break;

                case IChildFolder folder:
                    _scannedFolders.Add(folder);
                    break;

                default: return;
            }

            if (!Constants.Widgets.Health.ARE_UPDATES_OPTIMIZED)
                _progress.CallbackProgress?.Report(new(_scannedFolders.Count + _scannedFiles.Count, 0));
        }

        /// <inheritdoc/>
        public void Report(IResult value)
        {
            switch (value)
            {
                case IResult<StorableType> typeResult:
                {
                    if (typeResult.Value == StorableType.File)
                        Interlocked.Increment(ref _totalFilesScanned);
                    else if (typeResult.Value == StorableType.Folder)
                        Interlocked.Increment(ref _totalFoldersScanned);

                    break;
                }

                case IResult<IStorable> storableResult:
                {
                    if (storableResult.Value is IFile)
                        Interlocked.Increment(ref _totalFilesScanned);
                    else if (storableResult.Value is IFolder)
                        Interlocked.Increment(ref _totalFoldersScanned);

                    IssueFound?.Invoke(this, new(value, storableResult.Value as IStorableChild));
                    break;
                }

                default: return;
            }

            ReportProgress(_progress);
        }

        private async Task ScanStructureAsync(CancellationToken cancellationToken)
        {
            if (_structureValidator is null)
                return;

            if (Constants.Widgets.Health.IS_SCANNING_PARALLELIZED)
            {
                var tasks = new List<Task>(_scannedFolders.Count);
                foreach (var folder in _scannedFolders)
                    tasks.Add(ScanFolderAsync(folder, cancellationToken));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            else
            {
                foreach (var folder in _scannedFolders)
                    await ScanFolderAsync(folder, cancellationToken).ConfigureAwait(false);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            async Task ScanFolderAsync(IChildFolder folder, CancellationToken token)
            {
                // We do not care about the result, since we assume it is reported through IProgress
                _ = await _structureValidator.ValidateResultAsync((folder, this), token).ConfigureAwait(false);
            }
        }

        private void ReportProgress(ProgressModel<TotalProgress> progress)
        {
            if (Constants.Widgets.Health.ARE_UPDATES_OPTIMIZED)
            {
                _updateCount++;
                if (_updateCount < _updateInterval)
                    return;

                _updateCount = 0;
            }

            progress.PercentageProgress?.Report((double)(_totalFilesScanned + _totalFoldersScanned) / (_scannedFiles.Count + _scannedFolders.Count) * 100d);
            progress.CallbackProgress?.Report(new(_totalFilesScanned + _totalFoldersScanned, _scannedFiles.Count + _scannedFolders.Count));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _scannedFiles.Clear();
            _scannedFolders.Clear();
        }
    }
}
