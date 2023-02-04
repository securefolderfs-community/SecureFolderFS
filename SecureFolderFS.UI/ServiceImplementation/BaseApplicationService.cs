using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;
using System.Runtime.CompilerServices;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    public abstract class BaseApplicationService : IApplicationService
    {
        /// <inheritdoc/>
        public virtual string GetSystemVersion()
        {
            if (CompatibilityHelpers.IsPlatformWindows)
            {
                var windows = Environment.OSVersion;
                return $"Windows {windows.Version}";
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<LicenseModel> GetLicensesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                var licenses = licenseLink.Split(',').Select(x => new Uri(x));

                // Reset position and make sure cached data isn't combined when reading full text again
                stream.Position = 0L;
                streamReader.DiscardBufferedData();
                var fullText = await streamReader.ReadToEndAsync(cancellationToken);

                yield return new LicenseModel(packageName, licenses, licenseName, projectWebsiteUri, fullText);
            }
        }

        /// <inheritdoc/>
        public abstract AppVersion GetAppVersion();

        /// <inheritdoc/>
        public abstract Task OpenUriAsync(Uri uri);
    }
}
