using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.UI.Utils;
using static SecureFolderFS.Uno.PInvoke.UnsafeNative;

namespace SecureFolderFS.Uno.Helpers
{
    /// <summary>
    /// Plays short WAV assets embedded in SecureFolderFS.UI through whatever the host OS provides natively.
    /// </summary>
    internal static class SoundPlayerHelper
    {
        // Candidate CLI players in descending order of preference
        private static readonly string[][] _linuxPlayers =
        [
            ["paplay"],
            ["pw-play"],
            ["aplay", "-q"],
            ["ffplay", "-nodisp", "-autoexit", "-loglevel", "quiet"]
        ];

        private static readonly Lock _lock = new();

        private static Task? _prepareTask;
        private static string? _preparedResource;
        private static string? _preparedPath;
        private static uint _preparedSoundId;
        private static Process? _playerProcess;
#if WINDOWS
        private static GCHandle _pinnedSound;
        private static byte[]? _preparedBytes;
#endif

        /// <summary>
        /// Gets roughly how long the platform takes to get from <see cref="Play"/> to its first audible sample,
        /// assuming <see cref="PrepareAsync"/> has already completed.
        /// </summary>
        /// <remarks>
        /// This is dominated by opening an output device, not by anything to do with the asset. Windows mixes
        /// the buffer in-process and starts effectively at once. macOS goes through AudioToolbox, which once
        /// registered and primed measures at 18-60ms; the CLI players used on Linux pay their device-open cost
        /// per launch and cannot be primed, so they are given a much larger estimate.
        /// </remarks>
        public static TimeSpan StartupLatency { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? TimeSpan.Zero
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? TimeSpan.FromMilliseconds(60d)
                : TimeSpan.FromMilliseconds(150d);

        /// <summary>
        /// Extracts an embedded WAV asset and gets the platform ready to play it at a moment's notice.
        /// </summary>
        /// <param name="resourceName">The manifest resource name of the WAV asset.</param>
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
        /// <remarks>
        /// The wait is always at least <see cref="StartupLatency"/>, so the caller can rely on the sound being
        /// under way on return no matter what <paramref name="settle"/> is. Preparation is awaited before the
        /// timed section begins, leaving nothing on the clock but the play call itself.
        /// </remarks>
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
        /// <param name="resourceName">The manifest resource name of the WAV asset.</param>
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
                lock (_lock)
                {
#if WINDOWS
                    PlaySound(IntPtr.Zero, IntPtr.Zero, SND_PURGE);
                    ReleasePinnedSound();
#else
                    if (_playerProcess is { HasExited: false } process)
                        process.Kill(entireProcessTree: true);
#endif
                }
            }
            catch (Exception)
            {
                // Do nothing
            }
        }
        
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

#if WINDOWS
                lock (_lock)
                    _preparedBytes = bytes;

                return;
#endif

                // Both the CLI players and AudioToolbox address the sound by path rather than by buffer
                var path = Path.Combine(Path.GetTempPath(), $"sffs-{Guid.NewGuid():N}.wav");
                File.WriteAllBytes(path, bytes);

                string? previousPath;
                lock (_lock)
                {
                    previousPath = _preparedPath;
                    _preparedPath = path;
                }

                DeleteQuietly(previousPath);
                if (!OperatingSystem.IsMacOS())
                    return;

                var soundId = RegisterSystemSound(path);
                uint previousSoundId;
                lock (_lock)
                {
                    previousSoundId = _preparedSoundId;
                    _preparedSoundId = soundId;
                }

                if (previousSoundId != 0)
                    AudioServicesDisposeSystemSoundID(previousSoundId);

                if (soundId != 0)
                    PrimeOutputDevice();
            }
            catch (Exception)
            {
                // Do nothing
            }
        }
        
        private static void PlayPrepared()
        {
            try
            {
#if WINDOWS
                lock (_lock)
                {
                    if (_preparedBytes is not { } bytes)
                        return;

                    // Under SND_ASYNC winmm reads the buffer for the whole duration of playback,
                    // so it has to stay pinned until the sound is replaced or stopped
                    PlaySound(IntPtr.Zero, IntPtr.Zero, SND_PURGE);
                    ReleasePinnedSound();

                    _pinnedSound = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                    PlaySound(_pinnedSound.AddrOfPinnedObject(), IntPtr.Zero, SND_MEMORY | SND_ASYNC | SND_NODEFAULT);
                }

                return;
#endif

                uint soundId;
                string? path;
                lock (_lock)
                {
                    soundId = _preparedSoundId;
                    path = _preparedPath;
                }

                if (soundId != 0)
                {
                    AudioServicesPlaySystemSound(soundId);
                    return;
                }

                if (path is null)
                    return;

                var process = StartPlayer(path);
                if (process is null)
                    return;

                lock (_lock)
                {
                    _playerProcess = process;
                }
            }
            catch (Exception)
            {
                // Do nothing
            }
        }
        
#if __UNO_SKIA_MACOS__
        /// <summary>
        /// Registers a WAV file with the macOS system sound server, returning 0 if it would not take it.
        /// </summary>
        private static uint RegisterSystemSound(string path)
        {
            var cfPath = IntPtr.Zero;
            var cfUrl = IntPtr.Zero;
            try
            {
                cfPath = CFStringCreateWithCString(IntPtr.Zero, path, CF_STRING_ENCODING_UTF8);
                if (cfPath == IntPtr.Zero)
                    return 0;

                cfUrl = CFURLCreateWithFileSystemPath(IntPtr.Zero, cfPath, CF_URL_POSIX_PATH_STYLE, false);
                if (cfUrl == IntPtr.Zero)
                    return 0;

                return AudioServicesCreateSystemSoundID(cfUrl, out var soundId) == 0 ? soundId : 0;
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                if (cfUrl != IntPtr.Zero)
                    CFRelease(cfUrl);
                if (cfPath != IntPtr.Zero)
                    CFRelease(cfPath);
            }
        }
#endif

        /// <summary>
        /// Plays a fraction of a second of silence, purely to get the output device open.
        /// </summary>
        private static void PrimeOutputDevice()
        {
            var path = Path.Combine(Path.GetTempPath(), $"sffs-prime-{Guid.NewGuid():N}.wav");
            try
            {
                File.WriteAllBytes(path, CreateSilentWav(0.05d));

                var soundId = RegisterSystemSound(path);
                if (soundId == 0)
                    return;

                AudioServicesPlaySystemSound(soundId);
            }
            catch (Exception)
            {
                // Do nothing
            }
            finally
            {
                DeleteQuietly(path);
            }
        }

        /// <summary>
        /// Builds a mono 16-bit WAV of the given length containing nothing but silence.
        /// </summary>
        private static byte[] CreateSilentWav(double seconds)
        {
            const int sampleRate = 44100;
            var dataSize = (int)(sampleRate * seconds) * sizeof(short);

            using var buffer = new MemoryStream(44 + dataSize);
            using (var writer = new BinaryWriter(buffer, System.Text.Encoding.ASCII, leaveOpen: true))
            {
                writer.Write("RIFF"u8);
                writer.Write(36 + dataSize);
                writer.Write("WAVE"u8);
                writer.Write("fmt "u8);
                writer.Write(16);                       // PCM header length
                writer.Write((short)1);                 // PCM
                writer.Write((short)1);                 // mono
                writer.Write(sampleRate);
                writer.Write(sampleRate * sizeof(short));
                writer.Write((short)sizeof(short));     // block align
                writer.Write((short)16);                // bits per sample
                writer.Write("data"u8);
                writer.Write(dataSize);
                writer.Write(new byte[dataSize]);
            }

            return buffer.ToArray();
        }

        private static Process? StartPlayer(string path)
        {
            var candidates = OperatingSystem.IsMacOS() ? [["afplay"]] : _linuxPlayers;
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
                    if (Process.Start(startInfo) is { } process)
                        return process;
                }
                catch (Exception)
                {
                    // This player is not installed - fall through to the next candidate
                }
            }

            return null;
        }

        private static void DeleteQuietly(string? path)
        {
            if (path is null)
                return;

            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                // The file lives in the temp directory and will be reaped by the OS
            }
        }

#if WINDOWS
        private static void ReleasePinnedSound()
        {
            if (!_pinnedSound.IsAllocated)
                return;

            _pinnedSound.Free();
            _pinnedSound = default;
        }
#endif
    }
}
