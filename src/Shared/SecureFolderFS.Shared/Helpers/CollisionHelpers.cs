using System;
using System.Collections.Generic;
using System.IO;

namespace SecureFolderFS.Shared.Helpers
{
    public static class CollisionHelpers
    {
        /// <summary>
        /// Generates an available name that avoids collisions
        /// by appending a numeric suffix to the desired name
        /// if it already exists in the provided list of existing names.
        /// </summary>
        /// <param name="desiredName">The desired name that will be used if it does not already exist in the list of existing names.</param>
        /// <param name="existingNames">A collection of names that the desired name will be checked against to ensure uniqueness.</param>
        /// <param name="format">An optional format string that will be used to generate the new name. The default is "{0} ({1}){2}" where {0} is the base name, {1} is the counter, and {2} is the file extension.</param>
        /// <returns>A unique name that does not collide with any of the names in the existing collection.</returns>
        public static string GetAvailableName(string desiredName, IEnumerable<string> existingNames,
            string? format = null)
        {
            var existingNamesSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
            return GetAvailableName(desiredName, existingNamesSet, format);
        }

        /// <summary>
        /// Generates an available name that avoids collisions against a prebuilt, case-insensitive set of names.
        /// Use this overload in loops to avoid rebuilding the lookup for every item; add each returned
        /// name back to <paramref name="existingNamesSet"/> so subsequent calls account for it.
        /// </summary>
        /// <param name="desiredName">The desired name that will be used if it does not already exist in the set of existing names.</param>
        /// <param name="existingNamesSet">A set of names that the desired name will be checked against to ensure uniqueness. Should use <see cref="StringComparer.OrdinalIgnoreCase"/>.</param>
        /// <param name="format">An optional format string that will be used to generate the new name. The default is "{0} ({1}){2}" where {0} is the base name, {1} is the counter, and {2} is the file extension.</param>
        /// <returns>A unique name that does not collide with any of the names in the existing set.</returns>
        public static string GetAvailableName(string desiredName, HashSet<string> existingNamesSet,
            string? format = null)
        {
            if (!existingNamesSet.Contains(desiredName))
                return desiredName;

            var baseName = Path.GetFileNameWithoutExtension(desiredName);
            var extension = Path.GetExtension(desiredName);
            var counter = 1;
            format ??= "{0} ({1}){2}";

            string newName;
            do
            {
                newName = string.Format(format, baseName, counter, extension);
                counter++;
            } while (existingNamesSet.Contains(newName));

            return newName;
        }
    }
}
