using SecureFolderFS.Sdk.AppModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    // TODO: Needs docs
    public interface IChangelogService
    {
        Task<AppChangelog?> GetChangelogAsync(AppVersion version, CancellationToken cancellationToken);

        IAsyncEnumerable<AppChangelog> GetChangelogSinceAsync(AppVersion version, CancellationToken cancellationToken);
    }
}
