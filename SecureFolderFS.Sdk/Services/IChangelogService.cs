using SecureFolderFS.Sdk.DataModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    // TODO: Needs docs
    public interface IChangelogService
    {
        Task<ChangelogDataModel?> GetChangelogAsync(Version version, string platform, CancellationToken cancellationToken);

        IAsyncEnumerable<ChangelogDataModel> GetChangelogSinceAsync(Version version, string platform, CancellationToken cancellationToken);
    }
}
