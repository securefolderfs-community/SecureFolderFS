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
        Task<ChangelogDataModel> GetLatestAsync(Version version, CancellationToken cancellationToken);

        IAsyncEnumerable<ChangelogDataModel> GetSinceAsync(Version version, CancellationToken cancellationToken);
    }
}
