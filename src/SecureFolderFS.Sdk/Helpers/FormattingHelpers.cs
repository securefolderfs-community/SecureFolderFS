using System.IO;
using System.Linq;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class FormattingHelpers
    {
        public static string SanitizeVolumeName(string volumeName, string? fallback)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitizedVolumeName = new string(volumeName.Where(ch => !invalidChars.Contains(ch)).ToArray());

            // Ensure the sanitized name is not empty and has a minimum length
            if (string.IsNullOrWhiteSpace(sanitizedVolumeName) || sanitizedVolumeName.Length < 3)
                sanitizedVolumeName = fallback ?? "Mounted Volume";

            return sanitizedVolumeName;
        }
    }
}
