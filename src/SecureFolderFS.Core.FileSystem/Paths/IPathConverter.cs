using System;
using OwlCore.Storage;

namespace SecureFolderFS.Core.FileSystem.Paths
{
    /// <summary>
    /// Manages and converts paths between their ciphertext and plaintext forms.
    /// </summary>
    [Obsolete("Use INameCrypt")]
    public interface IPathConverter
    {
        [Obsolete]
        IFolder ContentFolder { get; }

        [Obsolete]
        string? ToCiphertext(string cleartextPath);

        [Obsolete]
        string? ToCleartext(string ciphertextPath);

        [Obsolete]
        string? GetCleartextFileName(string ciphertextFilePath);

        [Obsolete]
        string? GetCiphertextFileName(string cleartextFilePath);
    }
}
