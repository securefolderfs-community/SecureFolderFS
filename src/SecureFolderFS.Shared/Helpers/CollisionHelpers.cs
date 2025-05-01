using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Shared.Helpers
{
    public static class CollisionHelpers
    {
        public static async Task<string> GetAvailableNameAsync(this IFolder folder, string name, CancellationToken cancellationToken = default)
        {
            var baseName = Path.GetFileNameWithoutExtension(name);
            var extension = Path.GetExtension(name);
            var candidate = name;
            var counter = 1;

            var pattern = $@"^{Regex.Escape(baseName)}(?: \((\d+)\))?{Regex.Escape(extension)}$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);


            await Task.CompletedTask;
            return "";
        }

        public static string GetAvailableName(string folderPath, string name)
        {
            string baseName = Path.GetFileNameWithoutExtension(name);
            string extension = Path.GetExtension(name);
            string candidate = name;
            int counter = 1;

            // Create a regex to match files like "name", "name (1)", "name (2)", etc.
            string pattern = $@"^{Regex.Escape(baseName)}(?: \((\d+)\))?{Regex.Escape(extension)}$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            var existingNames = new HashSet<string>(
                Directory.EnumerateFileSystemEntries(folderPath),
                StringComparer.OrdinalIgnoreCase);

            while (existingNames.Contains(Path.Combine(folderPath, candidate)))
            {
                candidate = $"{baseName} ({counter}){extension}";
                counter++;
            }

            return candidate;
        }
    }
}