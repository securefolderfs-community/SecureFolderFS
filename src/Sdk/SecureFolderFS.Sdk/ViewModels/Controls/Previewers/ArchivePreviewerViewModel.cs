using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Sdk.ViewModels.Controls.Transfer;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Inject<IOverlayService>]
    [Bindable(true)]
    public sealed partial class ArchivePreviewerViewModel : FilePreviewerViewModel
    {
        private readonly FolderViewModel _folderViewModel;
        private readonly TransferViewModel? _transferViewModel;

        [ObservableProperty] private string? _Size;
        [ObservableProperty] private bool _IsSupported;
        [ObservableProperty] private bool _IsProgressing;

        public ArchivePreviewerViewModel(IFile file, FolderViewModel folderViewModel, TransferViewModel? transferViewModel = null)
            : base(file)
        {
            ServiceProvider = DI.Default;
            _folderViewModel = folderViewModel;
            _transferViewModel = transferViewModel;
            Title = file.Name;
            IsToolbarOnTop = true;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Only zip is supported via System.IO.Compression
            var extension = Path.GetExtension(Inner.Name).ToLowerInvariant();
            IsSupported = extension == ".zip";

            var size = await Inner.GetSizeAsync(cancellationToken);
            if (size is not null)
                Size = ByteSize.FromBytes(size.Value).ToString().Replace(" ", string.Empty);
        }

        [RelayCommand]
        private async Task ExtractAsync(CancellationToken cancellationToken)
        {
            if (!IsSupported || IsProgressing)
                return;

            IsProgressing = true;
            try
            {
                if (_transferViewModel is not null)
                {
                    _transferViewModel.TransferType = TransferType.Extract;
                    await _transferViewModel.PerformOperationAsync(async ct =>
                    {
                        await ExtractArchiveAsync(ct);
                    }, cancellationToken);
                }
                else
                {
                    await ExtractArchiveAsync(cancellationToken);
                }

                if (OverlayService.CurrentView is PreviewerOverlayViewModel { PreviewerViewModel: { } viewModel } && viewModel == this)
                    await OverlayService.CloseAllAsync();
            }
            catch (OperationCanceledException)
            {
                // User cancelled
            }
            catch (Exception)
            {
                // Extraction failed - silently handle
            }
            finally
            {
                IsProgressing = false;
            }
        }

        private async Task ExtractArchiveAsync(CancellationToken cancellationToken)
        {
            if (_folderViewModel.Folder is not IModifiableFolder modifiableFolder)
                return;

            await using var fileStream = await Inner.OpenReadAsync(cancellationToken);
            await using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

            // Track root-level items that have already been added to the UI
            var addedRootItems = new HashSet<string>();
            foreach (var entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip directory-only entries
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                // Determine the root-level name for this entry
                var topLevelParts = entry.FullName.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
                var isRootFile = topLevelParts.Length == 1;
                var rootName = topLevelParts[0];

                // For entries in subdirectories, create the folder hierarchy
                var targetFolder = modifiableFolder;
                var directoryPath = Path.GetDirectoryName(entry.FullName);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    var parts = directoryPath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i < parts.Length; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var subfolder = await targetFolder.CreateFolderAsync(parts[i], false, cancellationToken);
                        if (subfolder is not IModifiableFolder modifiableSubfolder)
                            break;

                        // Add the root-level folder to the UI on first encounter
                        if (i == 0 && addedRootItems.Add(rootName))
                        {
                            _folderViewModel.Items.Insert(
                                new FolderViewModel(subfolder, _folderViewModel.BrowserViewModel, _folderViewModel),
                                _folderViewModel.BrowserViewModel.Layouts.GetSorter());
                        }

                        targetFolder = modifiableSubfolder;
                    }
                }

                // Create the file and copy contents
                var newFile = await targetFolder.CreateFileAsync(entry.Name, true, cancellationToken);
                await using var entryStream = await entry.OpenAsync(cancellationToken);
                await using var destinationStream = await newFile.OpenWriteAsync(cancellationToken);
                await entryStream.CopyToAsync(destinationStream, cancellationToken);

                // Add root-level files to the UI
                if (isRootFile && addedRootItems.Add(rootName))
                {
                    _folderViewModel.Items.Insert(
                        new FileViewModel(newFile, _folderViewModel.BrowserViewModel, _folderViewModel),
                        _folderViewModel.BrowserViewModel.Layouts.GetSorter());
                }
            }
        }
    }
}