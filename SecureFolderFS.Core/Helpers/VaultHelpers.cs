using System;
using System.IO;
using System.Linq;

namespace SecureFolderFS.Core.Helpers
{
    internal static class VaultHelpers
    {
        public static char GetUnusedMountLetter()
        {
            var occupiedLetters = Directory.GetLogicalDrives().Select(item => item[0]);
            var availableLetters = Constants.ALPHABET.ToCharArray().Skip(3).Except(occupiedLetters); // Skip C and A, B - these are reserved for floppy disks and should not be used

            // Return first or default letter
            return availableLetters.FirstOrDefault(defaultValue: char.MinValue);
        }
    }
}
