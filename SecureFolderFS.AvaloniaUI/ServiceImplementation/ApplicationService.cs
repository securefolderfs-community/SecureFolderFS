using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class ApplicationService : IApplicationService
    {
        /// <inheritdoc/>
        public AppVersion GetAppVersion()
        {
            return new(Assembly.GetExecutingAssembly().GetName().Version!, "AvaloniaUI");
        }

        /// <inheritdoc/>
        public Task OpenUriAsync(Uri uri)
        {
            LauncherHelper.Launch(uri);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<LicenseModel> GetLicensesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "SecureFolderFS.UI")!;
            foreach (var item in assembly.GetManifestResourceNames().Where(resource => resource.StartsWith("SecureFolderFS.UI.Assets.Licenses")))
            {
                await using var stream = assembly.GetManifestResourceStream(item);
                using var sr = new StreamReader(stream, leaveOpen: true);

                var packageName = await sr.ReadLineAsync(cancellationToken);
                var licenseLink = await sr.ReadLineAsync(cancellationToken);
                var licenseName = await sr.ReadLineAsync(cancellationToken);
                var projectWebsite = await sr.ReadLineAsync(cancellationToken);
                var projectWebsiteUri = projectWebsite is null ? null : new Uri(projectWebsite);
                var licenses = licenseLink.Split(',').Select(x => new Uri(x));

                // Reset position and make sure cached data isn't combined when reading full text again
                stream.Position = 0L;
                sr.DiscardBufferedData();
                var fullText = await sr.ReadToEndAsync(cancellationToken);

                yield return new LicenseModel(packageName, licenses, licenseName, projectWebsiteUri, fullText);
            }
        }
    }
}