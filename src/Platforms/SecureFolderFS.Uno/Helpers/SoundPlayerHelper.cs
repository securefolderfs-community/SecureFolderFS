using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Uno.Helpers
{
    /// <summary>
    /// Plays short WAV assets embedded in SecureFolderFS.UI through whatever the host OS provides natively.
    /// </summary>
    internal static partial class SoundPlayerHelper
    {
        private static readonly Lock _lock = new();

        private static Task? _prepareTask;
        private static string? _preparedResource;
        private static string? _preparedPath;
        private static Process? _playerProcess;

        /// <summary>
        /// Gets roughly how long the platform takes to get from a play call to its first audible sample,
        /// assuming <see cref="PrepareAsync"/> has already completed.
        /// </summary>
        public static TimeSpan StartupLatency => PlatformStartupLatency;

        /// <summary>
        /// Extracts an embedded WAV asset and gets the platform ready to play it.
        /// </summary>
        /// <param name="resourceName">The manifest resource name of the WAV asset within SecureFolderFS.UI.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static Task PrepareAsync(string resourceName)
        {
            lock (_lock)
            {
                if (_prepareTask is not null && _preparedResource == resourceName)
                    return _prepareTask;

                _preparedResource = resourceName;
                _prepareTask = Task.Run(() => Prepare(resourceName));

                return _prepareTask;
            }
        }

        /// <summary>
        /// Plays an embedded WAV asset so that its first sample is audible once the returned task completes,
        /// having waited at least <paramref name="settle"/>.
        /// </summary>
        /// <param name="resourceName">The manifest resource name of the WAV asset within SecureFolderFS.UI.</param>
        /// <param name="settle">A minimum amount of time to wait, for the caller's own purposes.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task PlayAfterAsync(string resourceName, TimeSpan settle = default)
        {
            await PrepareAsync(resourceName);

            var lead = StartupLatency;
            if (settle > lead)
                await Task.Delay(settle - lead);

            PlayPrepared();
            await Task.Delay(lead);
        }

        /// <summary>
        /// Starts playing an embedded WAV asset and returns immediately.
        /// </summary>
        /// <param name="resourceName">The manifest resource name of the WAV asset within SecureFolderFS.UI.</param>
        public static void Play(string resourceName)
        {
            _ = PrepareAsync(resourceName).ContinueWith(_ => PlayPrepared(), TaskScheduler.Default);
        }

        /// <summary>
        /// Stops a sound that is still playing, where the platform allows it.
        /// </summary>
        public static void Stop()
        {
            try
            {
                StopPlatform();
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        /// <summary>
        /// Reads the asset and hands it to the backend to make ready.
        /// </summary>
        private static void Prepare(string resourceName)
        {
            try
            {
                byte[] bytes;
                using (var stream = typeof(IOverlayControl).Assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream is null)
                        return;

                    using var buffer = new MemoryStream();
                    stream.CopyTo(buffer);
                    bytes = buffer.ToArray();
                }

                PreparePlatform(bytes);
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        /// <summary>
        /// Starts whatever <see cref="Prepare"/> readied.
        /// </summary>
        private static void PlayPrepared()
        {
            try
            {
                PlayPlatform();
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        /// <summary>
        /// Writes the asset to a temporary file for the backends that address sounds by path.
        /// </summary>
        /// <returns>The path the asset was written to.</returns>
        private static string PrepareTempSound(byte[] bytes)
        {
            var path = Path.Combine(Path.GetTempPath(), $"sffs-{Guid.NewGuid():N}.wav");
            File.WriteAllBytes(path, bytes);

            string? previousPath;
            lock (_lock)
            {
                previousPath = _preparedPath;
                _preparedPath = path;
            }

            if (previousPath is not null)
                SafetyHelpers.NoFailure(() => File.Delete(previousPath));

            return path;
        }

        /// <summary>
        /// Launches the first available player from <paramref name="candidates"/>.
        /// </summary>
        private static bool StartPlayerProcess(string path, string[][] candidates)
        {
            foreach (var candidate in candidates)
            {
                var startInfo = new ProcessStartInfo(candidate[0])
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                for (var i = 1; i < candidate.Length; i++)
                    startInfo.ArgumentList.Add(candidate[i]);

                startInfo.ArgumentList.Add(path);

                try
                {
                    if (Process.Start(startInfo) is not { } process)
                        continue;

                    lock (_lock)
                    {
                        _playerProcess = process;
                    }

                    return true;
                }
                catch (Exception)
                {
                    // This player is not installed - fall through to the next candidate
                }
            }

            return false;
        }

        /// <summary>
        /// Kills the player started by <see cref="StartPlayerProcess"/>, if it is still running.
        /// </summary>
        private static void StopPlayerProcess()
        {
            lock (_lock)
            {
                if (_playerProcess is { HasExited: false } process)
                    process.Kill(entireProcessTree: true);
            }
        }
    }
}
