using System.IO;
using System.Linq;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class FormattingHelpers
    {
        private static char[] UniversalInvalidCharacters =
        {
            Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar,
            Path.PathSeparator, ':', '>', '<', '"', '?', '*', '!', '|', (char)0x0
        };

        public static string SanitizeItemName(string itemName, string fallback)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitizedItemName = new string(itemName.Where(c => !invalidChars.Contains(c) && !UniversalInvalidCharacters.Contains(c)).ToArray());

            // Ensure the sanitized name is not empty and has a minimum length
            if (string.IsNullOrWhiteSpace(sanitizedItemName))
                sanitizedItemName = SanitizeVolumeName(fallback, null);

            return sanitizedItemName;
        }

        public static string SanitizeVolumeName(string volumeName, string? fallback)
        {
            return SanitizeItemName(volumeName, fallback ?? "Mounted Volume");
        }
    }
}
