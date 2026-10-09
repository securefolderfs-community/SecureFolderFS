#if __UNO_SKIA_MACOS__
using System;
using System.IO;
using System.Text;
using SecureFolderFS.Shared.Helpers;
using static SecureFolderFS.Uno.PInvoke.UnsafeNative;

namespace SecureFolderFS.Uno.Helpers
{
    internal static partial class SoundPlayerHelper
    {
        // Only used if the sound cannot be registered; afplay is always present on macOS
        private static readonly string[][] _fallbackPlayers = [["afplay"]];
        private static uint _preparedSoundId;

        /// <inheritdoc cref="StartupLatency"/>
        /// <remarks>
        /// Measured at 18-60ms once registered and primed, against ~385ms for a fresh afplay each time.
        /// </remarks>
        private static TimeSpan PlatformStartupLatency => TimeSpan.FromMilliseconds(60d);

        private static void PreparePlatform(byte[] bytes)
        {
            // AudioToolbox addresses sounds by URL, so the asset has to exist on disk
            var path = PrepareTempSound(bytes);
            var soundId = RegisterSystemSound(path);

            uint previousSoundId;
            lock (_lock)
            {
                previousSoundId = _preparedSoundId;
                _preparedSoundId = soundId;
            }

            if (previousSoundId != 0)
                _ = AudioServicesDisposeSystemSoundID(previousSoundId);
            
            if (soundId != 0)
                PrimeOutputDevice();
        }

        private static void PlayPlatform()
        {
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

            // Registration failed
            if (path is not null)
                StartPlayerProcess(path, _fallbackPlayers);
        }

        private static void StopPlatform()
        {
            StopPlayerProcess();
        }

        /// <summary>
        /// Registers a WAV file with the macOS system sound server, returning 0 if it does not take it.
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

        /// <summary>
        /// Plays a fraction of a second of silence, to get the output device open.
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
                SafetyHelpers.NoFailure(() => File.Delete(path));
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
            using (var writer = new BinaryWriter(buffer, Encoding.ASCII, leaveOpen: true))
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
    }
}
#endif
