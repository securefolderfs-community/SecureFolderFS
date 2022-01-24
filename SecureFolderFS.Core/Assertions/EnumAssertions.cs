using System;
using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Core.Assertions
{
    internal static class EnumAssertions
    {
        public static void AssertCorrectContentCipherScheme(ContentCipherScheme contentCipherScheme)
        {
            if (contentCipherScheme == ContentCipherScheme.Undefined)
            {
                throw new ArgumentException($"Invalid flag for {nameof(contentCipherScheme)}.");
            }
        }

        public static void AssertCorrectFileNameCipherScheme(FileNameCipherScheme fileNameCipherScheme)
        {
            if (fileNameCipherScheme == FileNameCipherScheme.Undefined)
            {
                throw new ArgumentException($"Invalid flag for {nameof(fileNameCipherScheme)}.");
            }
        }
    }
}
