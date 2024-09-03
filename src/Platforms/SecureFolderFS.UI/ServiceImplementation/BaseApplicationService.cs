using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    public abstract class BaseApplicationService : IApplicationService
    {
        /// <inheritdoc/>
        public abstract bool IsDesktop { get; }

        /// <inheritdoc/>
        public abstract string Platform { get; }

        /// <inheritdoc/>
        public virtual Version AppVersion
        {
            get => Assembly.GetExecutingAssembly().GetName().Version!;
        }

        /// <inheritdoc/>
        public abstract string GetSystemVersion();

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<LicenseViewModel> GetLicensesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "SecureFolderFS.UI")!;
            foreach (var item in assembly.GetManifestResourceNames().Where(resource => resource.StartsWith("SecureFolderFS.UI.Assets.Licenses")))
            {
                await using var stream = assembly.GetManifestResourceStream(item);
                if (stream is null)
                    continue;

                using var streamReader = new StreamReader(stream, leaveOpen: true);

                var packageName = await streamReader.ReadLineAsync(cancellationToken) ?? string.Empty;
                var licenseLink = await streamReader.ReadLineAsync(cancellationToken) ?? string.Empty;
                var licenseName = await streamReader.ReadLineAsync(cancellationToken) ?? string.Empty;
                var projectWebsite = await streamReader.ReadLineAsync(cancellationToken);
                var projectWebsiteUri = projectWebsite is null ? null : new Uri(projectWebsite);
                var licenseUris = licenseLink.Split(',').Select(x => new Uri(x)).ToArray();

                // Reset position and make sure cached data isn't combined when reading full text again
                stream.Position = 0L;
                streamReader.DiscardBufferedData();
                var fullText = await streamReader.ReadToEndAsync(cancellationToken);

                yield return new()
                {
                    PackageName = packageName,
                    LicenseName = licenseName,
                    LicenseContent = fullText,
                    ProjectWebsiteUri = projectWebsiteUri,
                    LicenseUris = licenseUris
                };
            }
        }

        /// <inheritdoc/>
        public abstract Task OpenUriAsync(Uri uri);

        /// <inheritdoc/>
        public abstract Task TryRestartAsync();
    }
}
