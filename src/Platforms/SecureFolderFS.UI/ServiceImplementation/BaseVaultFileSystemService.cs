using System;
using System.Collections.Generic;
using System.IO;
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
                    EndOfStreamException => new HealthCascadingIssuesViewModel(storable, result, "Invalid directory ID") { ErrorMessage = "Regenerate invalid directory ID", Icon = "\uE8B7" }.WithInitAsync(),
                    FileNotFoundException => new HealthCascadingIssuesViewModel(storable, result, "Missing directory ID") { ErrorMessage = "Regenerate missing directory ID", Icon = "\uE8B7" }.WithInitAsync(),
                    { } ex => GetDefault(ex),
                    _ => GetDefault(aggregate)
                },
                FormatException => new HealthRenameIssueViewModel(storable, result, "Invalid item name") { ErrorMessage = "Choose a new name", Icon = "\uE8AC" },
                { } ex => GetDefault(ex)
            };

            HealthIssueViewModel GetDefault(Exception ex)
            {
                return new HealthIssueViewModel(storable, result, ex.GetType().ToString()) { Icon = "\uE783" };
            }
        }

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<IFileSystem> GetFileSystemsAsync(CancellationToken cancellationToken);
    }
}
