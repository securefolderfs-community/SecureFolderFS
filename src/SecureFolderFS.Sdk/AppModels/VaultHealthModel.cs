using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Scanners;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultHealthModel"/>
    public sealed class VaultHealthModel : IVaultHealthModel
    {
        private readonly List<IChildFile> _scannedFiles;
        private readonly List<IChildFolder> _scannedFolders;
        private volatile int _totalFilesScanned;
        private volatile int _totalFoldersScanned;

        /// <inheritdoc/>
        public IFolderScanner<IStorableChild> FolderScanner { get; }

        /// <inheritdoc/>
        public event EventHandler<IStorableChild>? IssueFound;

        public VaultHealthModel(IFolderScanner<IStorableChild> folderScanner)
        {
            FolderScanner = folderScanner;
            _scannedFiles = new();
            _scannedFolders = new();
        }

        /// <inheritdoc/>
        public async Task ScanAsync(ProgressModel progress, CancellationToken cancellationToken = default)
        {
            progress.PrecisionProgress?.Report(0d);
            _totalFilesScanned = 0;
            _totalFoldersScanned = 0;
            _scannedFolders.Clear();

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
                
                progress.CallbackProgress?.Report(new MessageResult(true, $"Collecting items ({_scannedFolders.Count})"));
            }

            await Task.WhenAll(ScanFilesAsync(progress, cancellationToken), ScanFoldersAsync(progress, cancellationToken));
        }

        private async Task ScanFilesAsync(ProgressModel progress, CancellationToken cancellationToken)
        {
            foreach (var item in _scannedFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                // TODO: Scan a file (perhaps contents?)
                _ = item;
                await Task.Delay(3);

                // Report progress
                Interlocked.Increment(ref _totalFoldersScanned);
                progress.PrecisionProgress?.Report((double)(_totalFilesScanned + _totalFoldersScanned) / (_scannedFiles.Count + _scannedFolders.Count) * 100d);
                progress.CallbackProgress?.Report(new MessageResult(true, $"Scanning items ({_totalFoldersScanned + _totalFilesScanned})"));
            }
        }

        private async Task ScanFoldersAsync(ProgressModel progress, CancellationToken cancellationToken)
        {
            foreach (var item in _scannedFolders)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                // TODO: Scan a folder (DirectoryID)
                _ = item;
                await Task.Delay(3);

                // Report progress
                Interlocked.Increment(ref _totalFilesScanned);
                progress.CallbackProgress?.Report(new MessageResult(true, $"Scanning items ({_totalFoldersScanned + _totalFilesScanned})"));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
