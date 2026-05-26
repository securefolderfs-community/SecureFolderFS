using System;

namespace SecureFolderFS.Core.FileSystem.Exceptions
{
    /// <summary>
    /// Exception thrown when a Base4K-encoded file name (.sffs) is not in NFC (Form C) normalized form.
    /// </summary>
    public sealed class NormalizationException : Exception
    {
        public NormalizationException(string fileName)
            : base($"Base4K file name is not NFC-normalized: {fileName}")
        {
        }
    }
}
