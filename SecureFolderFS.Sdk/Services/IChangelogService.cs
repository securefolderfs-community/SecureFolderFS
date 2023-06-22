using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.ViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    // TODO: Needs docs
    public interface IChangelogService
    {
        Task<ChangelogViewModel?> GetChangelogAsync(AppVersion version, CancellationToken cancellationToken);

        IAsyncEnumerable<ChangelogViewModel> GetChangelogSinceAsync(AppVersion version, CancellationToken cancellationToken);
    }
}
