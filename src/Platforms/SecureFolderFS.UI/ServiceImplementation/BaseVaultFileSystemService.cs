using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
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
        public async Task<HealthIssueViewModel?> GetIssueViewModelAsync(IResult result, IStorable storable, CancellationToken cancellation)
        {
            await Task.CompletedTask;
            if (result.Successful || result.Exception is null)
                return null;

            // TODO: Use custom implementations of the view model with options to resolve issues
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
        public Task ResolveIssuesAsync(IEnumerable<HealthIssueViewModel> issues, CancellationToken cancellationToken = default)
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

                    // We actually prioritize Directory ID issues first since the children
                    // of the affected directory also need to be renamed in one sweep
                    return issue is HealthDirectoryIssueViewModel ? ++pathComponents : pathComponents;
                })
                .ToArray();

            // TODO: Resolve issues
            _ = sortedIssues;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<IFileSystem> GetFileSystemsAsync(CancellationToken cancellationToken);
    }
}
