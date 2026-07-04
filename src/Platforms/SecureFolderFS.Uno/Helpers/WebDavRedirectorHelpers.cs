using System;
using System.Threading.Tasks;
#if WINDOWS
using System.Diagnostics;
using Microsoft.Win32;
#endif

namespace SecureFolderFS.Uno.Helpers
{
    /// <summary>
    /// Helpers for interacting with the Windows WebDAV redirector (WebClient service) configuration.
    /// </summary>
    public static class WebDavRedirectorHelpers
    {
        /// <summary>
        /// The maximum value accepted by the WebDAV redirector for the file size limit (about 4GB).
        /// </summary>
        public const uint MAX_FILE_SIZE_LIMIT = uint.MaxValue;

#if WINDOWS
        private const uint DEFAULT_FILE_SIZE_LIMIT = 50_000_000u;
        private const string WEBCLIENT_PARAMETERS_KEY = @"SYSTEM\CurrentControlSet\Services\WebClient\Parameters";
        private const string FILE_SIZE_LIMIT_VALUE = "FileSizeLimitInBytes";
#endif

        /// <summary>
        /// Gets the maximum size in bytes that the WebDAV redirector allows for file transfers.
        /// </summary>
        /// <returns>The configured limit in bytes, or null if it could not be determined.</returns>
        public static uint? GetFileSizeLimit()
        {
#if WINDOWS
            try
            {
                using var parametersKey = Registry.LocalMachine.OpenSubKey(WEBCLIENT_PARAMETERS_KEY);
                return parametersKey?.GetValue(FILE_SIZE_LIMIT_VALUE) switch
                {
                    int value when value < 0 => null,
                    int value => value > 0 ? unchecked((uint)value) : null,

                    // The WebDAV redirector falls back to the default limit when the value is not present
                    null => DEFAULT_FILE_SIZE_LIMIT,
                    _ => null
                };
            }
            catch (Exception)
            {
                return null;
            }
#else
            return null;
#endif
        }

        /// <summary>
        /// Attempts to raise the WebDAV redirector file size limit to the maximum allowed value.
        /// </summary>
        /// <remarks>
        /// Modifying the limit requires elevation - the user is presented with a UAC prompt.
        /// The new limit takes effect once the WebClient service is restarted.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is true if the limit was applied, otherwise false.</returns>
        public static async Task<bool> TrySetMaxFileSizeLimitAsync()
        {
#if WINDOWS
            try
            {
                using var process = Process.Start(new ProcessStartInfo()
                {
                    FileName = "reg.exe",
                    Arguments = $@"add HKLM\{WEBCLIENT_PARAMETERS_KEY} /v {FILE_SIZE_LIMIT_VALUE} /t REG_DWORD /d {MAX_FILE_SIZE_LIMIT} /f",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                if (process is null)
                    return false;

                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch (Exception)
            {
                // The user most likely declined the elevation prompt
                return false;
            }
#else
            return await Task.FromResult(false);
#endif
        }
    }
}
