using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Health;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.ViewModels.Health;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultHealthService"/>
    public class VaultHealthService : IVaultHealthService
    {
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
        public async Task ResolveIssuesAsync(IEnumerable<HealthIssueViewModel> issues, IDisposable contractOrRoot, IVaultHealthService.IssueDelegate? issueDelegate, CancellationToken cancellationToken = default)
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
    }
}
