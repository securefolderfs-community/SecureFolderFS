using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Scanners;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultHealthModel"/>
    [Inject<IVaultFileSystemService>]
    public sealed partial class VaultHealthModel : IVaultHealthModel
    {
        private readonly List<IChildFile> _scannedFiles;
        private readonly List<IChildFolder> _scannedFolders;
        private readonly IAsyncValidator<IFile> _fileValidator;
        private readonly IAsyncValidator<IFolder> _folderValidator;
        private readonly bool _isOptimized;
        private int _updateCount;
        private int _updateInterval;
        private volatile int _totalFilesScanned;
        private volatile int _totalFoldersScanned;

        /// <inheritdoc/>
        public IFolderScanner<IStorableChild> FolderScanner { get; }

        /// <inheritdoc/>
        public event EventHandler<IStorableChild>? IssueFound;

        public VaultHealthModel(IFolder vaultFolder, IFolderScanner<IStorableChild> folderScanner, bool isOptimized)
        {
            ServiceProvider = DI.Default;
            FolderScanner = folderScanner;
            _isOptimized = isOptimized;
            _scannedFiles = new();
            _scannedFolders = new();
            _fileValidator = VaultFileSystemService.GetFileValidator(vaultFolder);
            _folderValidator = VaultFileSystemService.GetFolderValidator(vaultFolder);
        }

        /// <inheritdoc/>
        public async Task ScanAsync(ProgressModel<TotalProgress> progress, CancellationToken cancellationToken = default)
        {
            progress.PrecisionProgress?.Report(0d);
            _totalFilesScanned = 0;
            _totalFoldersScanned = 0;
            _scannedFiles.Clear();
            _scannedFolders.Clear();

            // Start collecting items
            progress.CallbackProgress?.Report(new());

            await foreach (var item in FolderScanner.ScanFolderAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (item is IChildFile file)
                    _scannedFiles.Add(file);
                else if (item is IChildFolder folder)
                    _scannedFolders.Add(folder);
                else
                {
                    continue;
                }
                
                if (!_isOptimized)
                    progress.CallbackProgress?.Report(new(_scannedFolders.Count + _scannedFiles.Count, 0));
            }

            if (_isOptimized)
            {
                var totalUpdatesOptimized = (int)((_scannedFiles.Count + _scannedFolders.Count) * 0.2d);
                _updateInterval = (_scannedFiles.Count + _scannedFolders.Count) / totalUpdatesOptimized;
            }

            await Task.WhenAll(ScanFilesAsync(progress, cancellationToken), ScanFoldersAsync(progress, cancellationToken));
            GC.Collect();
        }

        private async Task ScanFilesAsync(ProgressModel<TotalProgress> progress, CancellationToken cancellationToken)
        {
            await Parallel.ForEachAsync(_scannedFiles, cancellationToken, async (file, token) =>
            {
                await ScanAsync<IChildFile>(file, _fileValidator, token);

                // Report progress
                Interlocked.Increment(ref _totalFilesScanned);
                ReportProgress(progress);
            });
        }

        private async Task ScanFoldersAsync(ProgressModel<TotalProgress> progress, CancellationToken cancellationToken)
        {
            await Parallel.ForEachAsync(_scannedFolders, cancellationToken, async (folder, token) =>
            {
                await ScanAsync<IChildFolder>(folder, _folderValidator, token);

                // Report progress
                Interlocked.Increment(ref _totalFoldersScanned);
                ReportProgress(progress);
            });
        }

        private async Task ScanAsync<TStorable>(TStorable storable, IAsyncValidator<TStorable> asyncValidator, CancellationToken cancellationToken)
            where TStorable : IStorableChild
        {
            var result = await asyncValidator.TryValidateAsync(storable, cancellationToken);
            if (!result.Successful)
                IssueFound?.Invoke(this, storable);
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

            progress.PrecisionProgress?.Report((double)(_totalFilesScanned + _totalFoldersScanned) / (_scannedFiles.Count + _scannedFolders.Count) * 100d);
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
