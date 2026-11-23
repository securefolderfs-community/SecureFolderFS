#if WINDOWS
using System;
using System.Runtime.InteropServices;
using static SecureFolderFS.Uno.PInvoke.UnsafeNative;

namespace SecureFolderFS.Uno.Helpers
{
    internal static partial class SoundPlayerHelper
    {
        private static GCHandle _pinnedSound;
        private static byte[]? _preparedBytes;

        /// <inheritdoc cref="StartupLatency"/>
        private static TimeSpan PlatformStartupLatency => TimeSpan.Zero;

        private static void PreparePlatform(byte[] bytes)
        {
            lock (_lock)
            {
                _preparedBytes = bytes;
            }
        }

        private static void PlayPlatform()
        {
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
        }

        private static void StopPlatform()
        {
            lock (_lock)
            {
                PlaySound(IntPtr.Zero, IntPtr.Zero, SND_PURGE);
                ReleasePinnedSound();
            }
        }

        private static void ReleasePinnedSound()
        {
            if (!_pinnedSound.IsAllocated)
                return;

            _pinnedSound.Free();
            _pinnedSound = default;
        }
    }
}

#endif
