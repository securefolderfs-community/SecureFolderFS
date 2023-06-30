using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    // TODO: Needs docs
    public interface IChangelogService
    {
        Task<ChangelogDataModel?> GetChangelogAsync(AppVersion version, CancellationToken cancellationToken);

        IAsyncEnumerable<ChangelogDataModel> GetChangelogSinceAsync(AppVersion version, CancellationToken cancellationToken);
    }
}
