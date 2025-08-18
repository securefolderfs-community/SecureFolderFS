using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OwlCore.Storage;

namespace SecureFolderFS.Shared.Helpers
{
    public static class CollisionHelpers
    {
        public static string GetAvailableName(string desiredName, IEnumerable<IStorable> existingItems)
        {
            var existingNames = new HashSet<string>(existingItems.Select(item => item.Name), StringComparer.OrdinalIgnoreCase);

            if (!existingNames.Contains(desiredName))
                return desiredName;

            var baseName = Path.GetFileNameWithoutExtension(desiredName);
            var extension = Path.GetExtension(desiredName);
            var counter = 1;

            string newName;
            do
            {
                newName = $"{baseName} ({counter}){extension}";
                counter++;
            } while (existingNames.Contains(newName));

            return newName;
        }
    }
}