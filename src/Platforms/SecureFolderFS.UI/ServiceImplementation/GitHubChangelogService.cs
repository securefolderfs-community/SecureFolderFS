using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IChangelogService"/>
    public sealed class GitHubChangelogService : IChangelogService
    {
        private IApplicationService ApplicationService { get; } = DI.Service<IApplicationService>();

        /// <inheritdoc/>
        public async Task<ChangelogDataModel?> GetChangelogAsync(Version version, string platform, CancellationToken cancellationToken)
        {
            const string repoName = Constants.GitHub.REPOSITORY_NAME;
            const string repoOwner = Constants.GitHub.REPOSITORY_OWNER;

            var client = new GitHubClient(new ProductHeaderValue(repoOwner));
            var release = await client.Repository.Release.Get(repoOwner, repoName, version.ToString());

            return new(release.Name, release.Body, version);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ChangelogDataModel> GetChangelogSinceAsync(Version version, string platform, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            const string repoName = Constants.GitHub.REPOSITORY_NAME;
            const string repoOwner = Constants.GitHub.REPOSITORY_OWNER;

            var client = new GitHubClient(new ProductHeaderValue(repoOwner));
            var releases = await client.Repository.Release.GetAll(repoOwner, repoName);
            var currentVersion = ApplicationService.AppVersion;

            foreach (var item in releases)
            {
                if (item.Draft)
                    continue;

                var formattedVersion = item.TagName.Replace("v", string.Empty, StringComparison.OrdinalIgnoreCase);
                var itemVersion = Version.Parse(formattedVersion);

                // 'itemVersion' must be the same or newer than 'version' as well as the same or older than 'currentVersion'
                if (itemVersion >= version && itemVersion <= currentVersion)
                    yield return new(item.Name, item.Body, itemVersion);
            }
        }
    }
}
