using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Results
{
    /// <inheritdoc cref="IResult{T}"/>
    public interface IHealthResult : IResult<IStorable>
    {
        /// <summary>
        /// Tries to resolve the health issue, if possible.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task TryResolveAsync(CancellationToken cancellationToken);
    }
}
