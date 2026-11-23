#if __UNO_SKIA_X11__
using System;

namespace SecureFolderFS.Uno.Helpers
{
    internal static partial class SoundPlayerHelper
    {
        // Candidate players in descending order of preference
        private static readonly string[][] _linuxPlayers =
        [
            ["paplay"],
            ["pw-play"],
            ["aplay", "-q"],
            ["ffplay", "-nodisp", "-autoexit", "-loglevel", "quiet"]
        ];

        /// <inheritdoc cref="StartupLatency"/>
        private static TimeSpan PlatformStartupLatency => TimeSpan.FromMilliseconds(150d);

        private static void PreparePlatform(byte[] bytes)
        {
            // The players all address the sound by path
            PrepareTempSound(bytes);
        }

        private static void PlayPlatform()
        {
            string? path;
            lock (_lock)
                path = _preparedPath;

            if (path is not null)
                StartPlayerProcess(path, _linuxPlayers);
        }

        private static void StopPlatform()
        {
            StopPlayerProcess();
        }
    }
}

#endif
