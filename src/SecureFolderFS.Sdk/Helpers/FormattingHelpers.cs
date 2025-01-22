using System.IO;
using System.Linq;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class FormattingHelpers
    {
        public static string SanitizeItemName(string itemName, string fallback)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitizedItemName = new string(itemName.Where(ch => !invalidChars.Contains(ch)).ToArray());

            // Ensure the sanitized name is not empty and has a minimum length
            if (string.IsNullOrWhiteSpace(sanitizedItemName) || sanitizedItemName.Length < 3)
                sanitizedItemName = SanitizeVolumeName(fallback, null);

            return sanitizedItemName;
        }

        public static string SanitizeVolumeName(string volumeName, string? fallback)
        {
            return SanitizeItemName(volumeName, fallback ?? "Mounted Volume");
        }
    }
}
