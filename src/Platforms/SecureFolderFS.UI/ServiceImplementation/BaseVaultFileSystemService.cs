using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Health;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.ViewModels.Health;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    public abstract class BaseVaultFileSystemService : IVaultFileSystemService
    {
        /// <inheritdoc/>
        public Task<IFileSystem> GetLocalFileSystemAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IFileSystem>(new LocalFileSystem());
        }

        /// <inheritdoc/>
        public async Task<HealthIssueViewModel?> GetIssueViewModelAsync(IResult result, IStorableChild storable, CancellationToken cancellation)
        {
            await Task.CompletedTask;
            if (result.Successful || result.Exception is null)
                return null;

            return result.Exception switch
            {
                AggregateException aggregate => aggregate.InnerException switch
                {
                    EndOfStreamException => new HealthDirectoryIssueViewModel(storable, result, "Invalid directory ID") { ErrorMessage = "Regenerate invalid directory ID", Icon = "\uE8B7" }.WithInitAsync(),
                    FileNotFoundException => new HealthDirectoryIssueViewModel(storable, result, "Missing directory ID") { ErrorMessage = "Regenerate missing directory ID", Icon = "\uE8B7" }.WithInitAsync(),
                    { } ex => GetDefault(ex),
                    _ => GetDefault(aggregate)
                },
                FormatException => new HealthNameIssueViewModel(storable, result, "Invalid item name") { ErrorMessage = "Choose a new name", Icon = "\uE8AC" },
                CryptographicException => new HealthFileDataIssueViewModel(storable, result, "Corrupted file contents") { ErrorMessage = "Overwrite affected file regions", Icon = "\uE74C" },
                { } ex => GetDefault(ex)
            };

            HealthIssueViewModel GetDefault(Exception ex)
            {
                return new HealthIssueViewModel(storable, result, ex.GetType().ToString()) { Icon = "\uE783" };
            }
        }

        /// <inheritdoc/>
        public async Task ResolveIssuesAsync(IEnumerable<HealthIssueViewModel> issues, IDisposable contractOrRoot, IVaultFileSystemService.IssueDelegate? issueDelegate, CancellationToken cancellationToken = default)
        {
            // Sort in the following order:
            /*
                1. Corrupted file contents (HealthFileDataIssueViewModel)
                2. Corrupted folder/file names (HealthNameIssueViewModel) and corrupted Directory ID (HealthDirectoryIssueViewModel) starting with items with most parents
            */
            var sortedIssues = issues
                .OrderByDescending(issue => issue is HealthFileDataIssueViewModel)
                .ThenBy(issue => issue is HealthNameIssueViewModel or HealthDirectoryIssueViewModel)
                .ThenByDescending(issue =>
                {
                    // Get the amount of items
                    var pathComponents = issue.Inner.Id.Split(Path.DirectorySeparatorChar).Length;

                    // We actually prioritize Directory ID issues first, since the children
                    // of the affected directory also need to be renamed in one sweep
                    return issue is HealthDirectoryIssueViewModel ? ++pathComponents : pathComponents;
                })
                .ToArray();

            if (contractOrRoot is IWrapper<FileSystemSpecifics> specificsWrapper)
            {
                foreach (var item in sortedIssues)
                {
                    var result = item switch
                    {
                        // Name issue
                        HealthNameIssueViewModel nameIssue => await HealthHelpers.RepairNameAsync(
                            nameIssue.Inner,
                            specificsWrapper.Inner,
                            FormattingHelpers.SanitizeItemName(nameIssue.ItemName ?? string.Empty, nameIssue.OriginalName),
                            cancellationToken),

                        // TODO: Implement repair for HealthFileDataIssueViewModel
                        HealthFileDataIssueViewModel dataIssue => Result.Failure(null),

                        // Directory ID issue
                        HealthDirectoryIssueViewModel directoryIssue => await HealthHelpers.RepairDirectoryAsync(
                            directoryIssue.Folder ?? throw new ArgumentNullException(nameof(HealthDirectoryIssueViewModel.Folder)),
                            specificsWrapper.Inner.Security,
                            cancellationToken),

                        // Default
                        _ => null
                    };

                    if (result is not null)
                        issueDelegate?.Invoke(item, result);
                }
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<RecycleBinItemViewModel> GetRecycleBinItemsAsync(IVFSRoot vfsRoot, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                throw new NotSupportedException($"The specified {nameof(IVFSRoot)} instance is not supported.");
            
            var specifics = specificsWrapper.Inner;
            var recycleBinFolder = await AbstractRecycleBinHelpers.GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            await foreach (var item in recycleBinFolder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (item.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    continue;

                var dataModel = await AbstractRecycleBinHelpers.GetItemDataModelAsync(item, recycleBinFolder, specifics, StreamSerializer.Instance, cancellationToken);
                yield return new(item)
                {
                    Title = item.Name,
                    DeletionTimestamp = dataModel.DeletionTimestamp
                };
            }
        }

        /// <inheritdoc/>
        public async Task RestoreItemAsync(IVFSRoot vfsRoot, IStorableChild recycleBinItem, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                return;
            
            var specifics = specificsWrapper.Inner;
            var destinationFolder = await AbstractRecycleBinHelpers.GetDestinationFolderAsync(
                recycleBinItem,
                specifics,
                StreamSerializer.Instance,
                cancellationToken);

            // Prompt the user to pick the folder when the default destination couldn't be used
            if (destinationFolder is null)
            {
                // TODO: Add starting directory parameter
                var fileExplorerService = DI.Service<IFileExplorerService>();
                destinationFolder = await fileExplorerService.PickFolderAsync(false, cancellationToken) as IModifiableFolder;
                if (destinationFolder is null)
                    return;

                if (!destinationFolder.Id.Contains(vfsRoot.Inner.Id, StringComparison.OrdinalIgnoreCase))
                {
                    // Invalid folder chosen outside of vault
                    // TODO: Return IResult or throw
                    return;
                }
            }

            // Restore the item to chosen destination
            await AbstractRecycleBinHelpers.RestoreAsync(
                recycleBinItem,
                destinationFolder,
                specifics,
                StreamSerializer.Instance,
                cancellationToken);
        }

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<IFileSystem> GetFileSystemsAsync(CancellationToken cancellationToken);
    }
}
