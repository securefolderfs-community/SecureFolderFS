using System;
using System.Collections.Generic;
using System.IO;

namespace SecureFolderFS.Shared.Helpers
{
    public static class CollisionHelpers
    {
        public static string GetAvailableName(string desiredName, IEnumerable<string> existingNames)
        {
            var existingNamesSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
            if (!existingNamesSet.Contains(desiredName))
                return desiredName;

            var baseName = Path.GetFileNameWithoutExtension(desiredName);
            var extension = Path.GetExtension(desiredName);
            var counter = 1;

            string newName;
            do
            {
                newName = $"{baseName} ({counter}){extension}";
                counter++;
            } while (existingNamesSet.Contains(newName));

            return newName;
        }
    }
}