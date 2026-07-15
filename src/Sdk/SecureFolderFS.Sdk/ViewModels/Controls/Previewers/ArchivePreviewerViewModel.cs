using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
using SecureFolderFS.Shared.Helpers;
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
                Size = ByteSize.FromBytes(size.Value).ToString();
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
            catch (Exception ex)
            {
                if (_transferViewModel is not null)
                    await _transferViewModel.ReportErrorAsync($"{"OperationFailed".ToLocalized()} ({ex.Message})");
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

            // Map each root-level name in the archive to a collision-free name in the destination,
            // so extraction never silently merges with or overwrites existing items
            var existingNames = new HashSet<string>(_folderViewModel.Items.Select(x => x.Inner.Name), StringComparer.OrdinalIgnoreCase);
            var rootNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Track root-level items that have already been added to the UI
            var addedRootItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip directory-only entries
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                var parts = entry.FullName.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);

                // Guard against zip-slip: skip entries that could escape the destination folder
                if (parts.Length == 0 || Array.Exists(parts, static part => part is ".." or "."))
                    continue;

                // Resolve the collision-free root name once per distinct root
                var rootName = parts[0];
                if (!rootNameMap.TryGetValue(rootName, out var mappedRootName))
                {
                    mappedRootName = CollisionHelpers.GetAvailableName(rootName, existingNames);
                    rootNameMap[rootName] = mappedRootName;
                    existingNames.Add(mappedRootName);
                }

                // Root-level file: create it under the mapped (collision-free) name
                if (parts.Length == 1)
                {
                    var rootFile = await modifiableFolder.CreateFileAsync(mappedRootName, false, cancellationToken);
                    await CopyEntryAsync(entry, rootFile, cancellationToken);

                    if (addedRootItems.Add(mappedRootName))
                    {
                        _folderViewModel.Items.Insert(
                            new FileViewModel(rootFile, _folderViewModel.BrowserViewModel, _folderViewModel),
                            _folderViewModel.BrowserViewModel.Layouts.GetSorter());
                    }

                    continue;
                }

                // Create the folder hierarchy; the root folder uses the mapped name, so the
                // subtree is guaranteed fresh and cannot clobber pre-existing content
                var targetFolder = modifiableFolder;
                var hierarchyCreated = true;
                for (var i = 0; i < parts.Length - 1; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var folderName = i == 0 ? mappedRootName : parts[i];
                    var subfolder = await targetFolder.CreateFolderAsync(folderName, false, cancellationToken);
                    if (subfolder is not IModifiableFolder modifiableSubfolder)
                    {
                        hierarchyCreated = false;
                        break;
                    }

                    // Add the root-level folder to the UI on first encounter
                    if (i == 0 && addedRootItems.Add(mappedRootName))
                    {
                        _folderViewModel.Items.Insert(
                            new FolderViewModel(subfolder, _folderViewModel.BrowserViewModel, _folderViewModel),
                            _folderViewModel.BrowserViewModel.Layouts.GetSorter());
                    }

                    targetFolder = modifiableSubfolder;
                }

                if (!hierarchyCreated)
                    continue;

                var newFile = await targetFolder.CreateFileAsync(entry.Name, true, cancellationToken);
                await CopyEntryAsync(entry, newFile, cancellationToken);
            }
        }

        private static async Task CopyEntryAsync(ZipArchiveEntry entry, IFile destinationFile, CancellationToken cancellationToken)
        {
            await using var entryStream = await entry.OpenAsync(cancellationToken);
            await using var destinationStream = await destinationFile.OpenWriteAsync(cancellationToken);
            await entryStream.CopyToAsync(destinationStream, cancellationToken);
        }
    }
}