using OwlCore.Storage;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
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
    public sealed class HealthModel : IHealthModel
    {
        private readonly List<IChildFile> _scannedFiles;
        private readonly List<IChildFolder> _scannedFolders;
        private readonly IAsyncValidator<IFile, IResult>? _fileValidator;
        private readonly IAsyncValidator<IFolder, IResult>? _folderValidator;
        private readonly bool _isOptimized;
        private readonly bool _isParallelized;
        private int _updateCount;
        private int _updateInterval;
        private volatile int _totalFilesScanned;
        private volatile int _totalFoldersScanned;

        /// <inheritdoc/>
        public IFolderScanner FolderScanner { get; }

        /// <inheritdoc/>
        public event EventHandler<HealthIssueEventArgs>? IssueFound;

        public HealthModel(IFolderScanner folderScanner, IAsyncValidator<IFile, IResult>? fileValidator, IAsyncValidator<IFolder, IResult>? folderValidator)
        {
            FolderScanner = folderScanner;
            _isOptimized = true;
            _isParallelized = false;
            _scannedFiles = new();
            _scannedFolders = new();
            _fileValidator = fileValidator;
            _folderValidator = folderValidator;
        }

        /// <inheritdoc/>
        public async Task ScanAsync(ProgressModel<TotalProgress> progress, CancellationToken cancellationToken = default)
        {
            await Task.Run(async () =>
            {
                progress.PercentageProgress?.Report(0d);

                _totalFilesScanned = 0;
                _totalFoldersScanned = 0;
                _scannedFiles.Clear();
                _scannedFolders.Clear();

                // Start collecting items
                progress.CallbackProgress?.Report(new());
                await Task.Delay(750, cancellationToken);

                await foreach (var item in FolderScanner.ScanFolderAsync(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    switch (item)
                    {
                        case IChildFile file:
                            _scannedFiles.Add(file);
                            break;

                        case IChildFolder folder:
                            _scannedFolders.Add(folder);
                            break;

                        default:
                            continue;
                    }

                    if (!_isOptimized)
                        progress.CallbackProgress?.Report(new(_scannedFolders.Count + _scannedFiles.Count, 0));
                }

                if (_isOptimized)
                {
                    var totalUpdatesOptimized = (int)((_scannedFiles.Count + _scannedFolders.Count) * 0.2d);
                    _updateInterval = (_scannedFiles.Count + _scannedFolders.Count) / Math.Max(100, totalUpdatesOptimized);
                }

                // Report initial progress
                ReportProgress(progress);
                await Task.Delay(750, cancellationToken);

                if (!_isParallelized)
                {
                    await ScanFilesAsync(progress, cancellationToken);
                    await ScanFoldersAsync(progress, cancellationToken);
                }
                else
                    await Task.WhenAll(ScanFilesAsync(progress, cancellationToken), ScanFoldersAsync(progress, cancellationToken));

                // Report final progress
                ReportProgress(progress);
                await Task.Delay(1500, cancellationToken);
            }, cancellationToken);
        }

        private async Task ScanFilesAsync(ProgressModel<TotalProgress> progress, CancellationToken cancellationToken)
        {
            if (_fileValidator is null)
                return;

            if (_isParallelized)
            {
                await Parallel.ForEachAsync(_scannedFiles, cancellationToken, async (file, token) =>
                    await ScanFileAsync(file, token));
            }
            else
            {
                foreach (var file in _scannedFiles)
                    await ScanFileAsync(file, cancellationToken);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            async Task ScanFileAsync(IChildFile file, CancellationToken token)
            {
                await ScanAsync<IChildFile>(file, _fileValidator!, token);

                // Report progress
                Interlocked.Increment(ref _totalFilesScanned);
                ReportProgress(progress);
            }
        }

        private async Task ScanFoldersAsync(ProgressModel<TotalProgress> progress, CancellationToken cancellationToken)
        {
            if (_folderValidator is null)
                return;

            if (_isParallelized)
            {
                await Parallel.ForEachAsync(_scannedFolders, cancellationToken, async (folder, token) =>
                    await ScanFolderAsync(folder, token));
            }
            else
            {
                foreach (var folder in _scannedFolders)
                    await ScanFolderAsync(folder, cancellationToken);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            async Task ScanFolderAsync(IChildFolder folder, CancellationToken token)
            {
                await ScanAsync<IChildFolder>(folder, _folderValidator!, token);

                // Report progress
                Interlocked.Increment(ref _totalFoldersScanned);
                ReportProgress(progress);
            }
        }

        private async Task ScanAsync<TStorable>(TStorable storable, IAsyncValidator<TStorable, IResult> asyncValidator, CancellationToken cancellationToken)
            where TStorable : IStorableChild
        {
            var result = await asyncValidator.ValidateResultAsync(storable, cancellationToken);
            if (!result.Successful)
                IssueFound?.Invoke(this, new(storable, result));
        }

        private void ReportProgress(ProgressModel<TotalProgress> progress)
        {
            if (_isOptimized)
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
