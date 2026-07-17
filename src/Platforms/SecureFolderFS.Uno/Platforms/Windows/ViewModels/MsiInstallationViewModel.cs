using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.UI;

namespace SecureFolderFS.Uno.Platforms.Windows.ViewModels
{
    /// <summary>
    /// Downloads an MSI installer asset from a GitHub release and installs it silently through msiexec.
    /// </summary>
    public abstract partial class MsiInstallationViewModel(string id, string? title) : ItemInstallationViewModel(id, title)
    {
        private const int ERROR_CANCELLED = 1223; // The user dismissed the UAC prompt
        private const int ERROR_SUCCESS_REBOOT_REQUIRED = 3010;
        private const int DOWNLOAD_BUFFER_SIZE = 81920;

        /// <summary>
        /// Gets the owner of the GitHub repository to download the installer from.
        /// </summary>
        protected abstract string RepositoryOwner { get; }

        /// <summary>
        /// Gets the name of the GitHub repository to download the installer from.
        /// </summary>
        protected abstract string RepositoryName { get; }

        /// <summary>
        /// Gets the tag of the release to install.
        /// </summary>
        protected abstract string VersionTag { get; }

        /// <summary>
        /// Picks the installer asset appropriate for this machine, or null when the release has none.
        /// </summary>
        protected abstract ReleaseAsset? SelectInstallerAsset(IReadOnlyList<ReleaseAsset> assets);

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var mediaService = DI.Service<IMediaService>();
            Icon = await mediaService.GetImageFromResourceAsync(Id, cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task InstallAsync(CancellationToken cancellationToken)
        {
            SystemFolderEx? tempFolder = null;
            IChildFolder? installerFolder = null;
            try
            {
                IsProgressing = true;
                IsIndeterminate = true;

                var downloadUrl = await GetInstallerUrlAsync(cancellationToken);

                // A uniquely-named subfolder keeps the installer path unpredictable
                // to other local processes before it is executed elevated
                tempFolder = new SystemFolderEx(Path.GetTempPath());
                installerFolder = await tempFolder.CreateFolderAsync($"{nameof(SecureFolderFS)}_{Guid.NewGuid():N}", false, cancellationToken);
                if (installerFolder is not IModifiableFolder modifiableFolder)
                    throw new InvalidOperationException("The temporary installer folder is not modifiable.");

                IsIndeterminate = false;
                var installerFile = await DownloadFileAsync(downloadUrl, modifiableFolder, cancellationToken);

                IsIndeterminate = true;
                await RunInstallerSilentlyAsync(installerFile, cancellationToken);
                IsInstalled = true;
            }
            catch (OperationCanceledException)
            {
                // Cancellation, nothing to report
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == ERROR_CANCELLED)
            {
                // The user dismissed the UAC prompt - treat like cancellation
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
            finally
            {
                if (tempFolder is not null && installerFolder is not null)
                    _ = DeleteQuietlyAsync(tempFolder, installerFolder);

                IsProgressing = false;
                IsIndeterminate = false;
            }

            static async Task DeleteQuietlyAsync(IModifiableFolder parent, IStorableChild child)
            {
                try
                {
                    await parent.DeleteAsync(child, CancellationToken.None);
                }
                catch (Exception)
                {
                    // Best-effort cleanup of the temporary installer
                }
            }
        }

        private async Task<string> GetInstallerUrlAsync(CancellationToken cancellationToken)
        {
            var github = new GitHubClient(new ProductHeaderValue(Constants.GitHub.REPOSITORY_OWNER));
            var release = await github.Repository.Release.Get(RepositoryOwner, RepositoryName, VersionTag).WaitAsync(cancellationToken);
            var asset = SelectInstallerAsset(release.Assets);

            return asset?.BrowserDownloadUrl
                   ?? throw new InvalidOperationException($"No installer found in the {RepositoryName} '{VersionTag}' release.");
        }

        private async Task<IChildFile> DownloadFileAsync(string url, IModifiableFolder installerFolder, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var downloadedFile = await installerFolder.CreateFileAsync(Path.GetFileName(url), true, cancellationToken);
            await using var downloadedFileStream = await downloadedFile.OpenReadWriteAsync(cancellationToken);

            var buffer = new byte[DOWNLOAD_BUFFER_SIZE];
            var downloadedBytes = 0L;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await downloadedFileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                downloadedBytes += bytesRead;

                if (totalBytes > 0)
                    Report(downloadedBytes / (double)totalBytes * 100.0);
                else
                    IsIndeterminate = true; // Content-Length unavailable, can't track progress
            }

            return downloadedFile;
        }

        private async Task RunInstallerSilentlyAsync(IFile installerFile, CancellationToken cancellationToken)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "msiexec.exe",
                Arguments = $"/i \"{installerFile.Id}\" /quiet /norestart",
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo)
                                ?? throw new InvalidOperationException($"Failed to start the {Title} installer process.");

            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode is not (0 or ERROR_SUCCESS_REBOOT_REQUIRED))
                throw new InvalidOperationException($"The {Title} installer exited with code {process.ExitCode}.");
        }
    }
}
