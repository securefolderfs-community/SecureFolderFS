using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Api.Models;

namespace SecureFolderFS.Sdk.Api.Services
{
    /// <summary>
    /// Asks the user whether an application may connect.
    /// </summary>
    public interface IApiConsentService
    {
        /// <summary>
        /// Shows a consent prompt and waits for the user's decision.
        /// </summary>
        /// <param name="request">The consent request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the user's answer.</returns>'
        Task<ApiConsentResult> RequestConsentAsync(ApiConsentRequest request, CancellationToken cancellationToken = default);
    }
}
