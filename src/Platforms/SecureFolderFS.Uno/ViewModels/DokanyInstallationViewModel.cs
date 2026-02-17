using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using OwlCore.Storage;
using SecureFolderFS.UI;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.SystemStorageEx;

namespace SecureFolderFS.Uno.ViewModels
{
    public sealed partial class DokanyInstallationViewModel() : ItemInstallationViewModel("DOKANY", "Dokany")
    {
        private const string OWNER = "dokan-dev";
        private const string REPO = "dokany";

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var mediaService = DI.Service<IMediaService>();
            Icon = await mediaService.GetImageFromResourceAsync("Dokany", cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task InstallAsync(CancellationToken cancellationToken)
        {
            try
            {
                IsIndeterminate = true;

                var (downloadUrl, fileName) = await GetInstallerAssetAsync(cancellationToken);
                var installerFolder = new SystemFolderEx(Path.GetTempPath());

                IsIndeterminate = false;
                var downloadedFile = await DownloadFileAsync(downloadUrl, installerFolder, cancellationToken);

                IsIndeterminate = true;
                await RunInstallerSilentlyAsync(downloadedFile, cancellationToken);

                // Delete installed file
                await installerFolder.DeleteAsync(downloadedFile, cancellationToken);
                IsInstalled = true;
            }
            finally
            {
                IsProgressing = false;
                IsIndeterminate = false;
            }
        }

        private async Task<(string downloadUrl, string fileName)> GetInstallerAssetAsync(CancellationToken cancellationToken)
        {
            var github = new GitHubClient(new ProductHeaderValue(Constants.GitHub.REPOSITORY_OWNER));
            var release = await github.Repository.Release.Get(OWNER, REPO, "v2.3.1.1000").WaitAsync(cancellationToken);
            var archTag = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                Architecture.Arm64 => "arm64",
                _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
            };

            ReleaseAsset? asset = null;
            foreach (var item in release.Assets)
            {
                var name = item.Name.ToLowerInvariant();
                if (!name.EndsWith(".msi") || !name.Contains(archTag))
                    continue;

                asset = item;
                break;
            }

            return asset is not null
                ? (asset.BrowserDownloadUrl, asset.Name)
                : throw new InvalidOperationException($"No MSI installer found for architecture '{archTag}' in the Dokany release.");
        }

        private async Task<IChildFile> DownloadFileAsync(string url, IModifiableFolder installerFolder, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var downloadedFile = await installerFolder.CreateFileAsync($"{nameof(SecureFolderFS)}_{Path.GetFileName(url)}", true, cancellationToken);
            await using var downloadedFileStream = await downloadedFile.OpenReadWriteAsync(cancellationToken);

            var buffer = new byte[4096];
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

        private static async Task RunInstallerSilentlyAsync(IFile downloadFile, CancellationToken cancellationToken)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "msiexec.exe",
                Arguments = $"/i \"{downloadFile.Id}\" /quiet /norestart",
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo) 
                                ?? throw new InvalidOperationException("Failed to start the Dokany installer process.");

            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode is not (0 or 3010))
                throw new InvalidOperationException($"Dokany installer exited with code {process.ExitCode}.");
        }
    }
}
