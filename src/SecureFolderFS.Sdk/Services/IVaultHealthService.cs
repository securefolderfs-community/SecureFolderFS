using OwlCore.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Provides methods for managing and resolving health issues in a vault.
    /// </summary>
    public interface IVaultHealthService
    {
        /// <summary>
        /// Represents a delegate for handling health issues.
        /// </summary>
        /// <param name="issueViewModel">The health issue view model.</param>
        /// <param name="result">The result associated with the issue.</param>
        public delegate void IssueDelegate(HealthIssueViewModel issueViewModel, IResult result);

        /// <summary>
        /// Gets the <see cref="HealthIssueViewModel"/> implementation for the associated <see cref="IResult"/> from item validation.
        /// </summary>
        /// <param name="result">The result of validation.</param>
        /// <param name="storable">The affected storable item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If available, the value is a <see cref="HealthIssueViewModel"/>; otherwise, null.</returns>
        Task<HealthIssueViewModel?> GetIssueViewModelAsync(IResult result, IStorableChild storable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves the specified health issues.
        /// </summary>
        /// <param name="issues">The collection of health issues to resolve.</param>
        /// <param name="contractOrRoot">The disposable contract or root object associated with the resolution process.</param>
        /// <param name="issueDelegate">An optional delegate to handle individual issues during resolution.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ResolveIssuesAsync(IEnumerable<HealthIssueViewModel> issues, IDisposable contractOrRoot, IssueDelegate? issueDelegate, CancellationToken cancellationToken = default);
    }
}
