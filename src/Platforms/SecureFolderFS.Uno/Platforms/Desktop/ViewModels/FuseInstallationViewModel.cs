using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Models;
using Tmds.Fuse;

namespace SecureFolderFS.Uno.Platforms.Desktop.ViewModels
{
    /// <summary>
    /// Installs the FUSE userspace components (libfuse3 and fusermount3) through the
    /// distribution's native package manager, elevated with a PolicyKit (pkexec) prompt.
    /// The kernel half of FUSE ships with every mainstream distribution, so the 'fuse3'
    /// package is all that is needed to make <see cref="Core.FUSE.FuseFileSystem"/> available.
    /// </summary>
    public sealed partial class FuseInstallationViewModel() : ItemInstallationViewModel(Core.FUSE.Constants.FileSystem.FS_ID, Core.FUSE.Constants.FileSystem.FS_NAME)
    {
        private const int PKEXEC_DIALOG_DISMISSED = 126; // The user closed the authentication dialog
        private const int PKEXEC_NOT_AUTHORIZED = 127;

        // Package manager binaries and the arguments that install libfuse3 + fusermount3
        private static readonly (string FileName, string Arguments)[] KnownPackageManagers =
        [
            ("apt-get", "install -y fuse3"),
            ("dnf", "install -y fuse3"),
            ("yum", "install -y fuse3"),
            ("pacman", "-S --needed --noconfirm fuse3"),
            ("zypper", "--non-interactive install fuse3")
        ];

        /// <summary>
        /// Gets whether the app runs inside a Flatpak sandbox, where host packages cannot
        /// be installed and sandbox-local FUSE mounts would not be visible to the host.
        /// </summary>
        public static bool IsSandboxed { get; } = File.Exists("/.flatpak-info");

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var mediaService = DI.Service<IMediaService>();
            Icon = await mediaService.GetImageFromResourceAsync(Id, cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task InstallAsync(CancellationToken cancellationToken)
        {
            try
            {
                IsProgressing = true;
                IsIndeterminate = true; // Package managers don't expose machine-readable progress

                if (FindProgram("pkexec") is null)
                    throw new InvalidOperationException("PolicyKit (pkexec) was not found. Install the 'fuse3' package manually with your package manager.");

                var (packageManager, arguments) = FindInstallCommand()
                    ?? throw new InvalidOperationException("No supported package manager was found. Install the 'fuse3' package manually.");

                var exitCode = await RunElevatedAsync(packageManager, arguments, cancellationToken);
                switch (exitCode)
                {
                    case 0:
                        break;

                    case PKEXEC_DIALOG_DISMISSED:
                        return; // Dismissing the authentication dialog is deliberate - nothing to report

                    case PKEXEC_NOT_AUTHORIZED:
                        throw new UnauthorizedAccessException("Authorization to install the 'fuse3' package was not granted.");

                    default:
                        throw new InvalidOperationException($"The 'fuse3' package installation exited with code {exitCode}.");
                }

                // Confirm that libfuse3 and fusermount3 are actually usable now
                if (!Fuse.CheckDependencies())
                    throw new InvalidOperationException("FUSE is still unavailable after the installation.");

                IsInstalled = true;
            }
            catch (OperationCanceledException)
            {
                // Cancellation, nothing to report
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
            finally
            {
                IsProgressing = false;
                IsIndeterminate = false;
            }
        }

        private static (string FileName, string Arguments)? FindInstallCommand()
        {
            foreach (var (fileName, arguments) in KnownPackageManagers)
            {
                // pkexec resolves programs with its own restricted PATH, so pass the absolute path
                if (FindProgram(fileName) is { } fullPath)
                    return (fullPath, arguments);
            }

            return null;
        }

        private static string? FindProgram(string fileName)
        {
            var pathDirectories = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            // Also probe the standard locations in case the app was launched with a minimal PATH
            string[] fallbackDirectories = ["/usr/bin", "/usr/sbin", "/bin", "/sbin"];

            foreach (var directory in (string[])[.. pathDirectories, .. fallbackDirectories])
            {
                var candidate = Path.Combine(directory, fileName);
                if (File.Exists(candidate))
                    return candidate;
            }

            return null;
        }

        private static async Task<int> RunElevatedAsync(string fileName, string arguments, CancellationToken cancellationToken)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "pkexec",
                Arguments = $"{fileName} {arguments}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo)
                                ?? throw new InvalidOperationException("Failed to start the elevated installation process.");

            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode;
        }
    }
}
