namespace SecureFolderFS.Core.FileSystem.Paths
{
    /// <summary>
    /// Manages and converts paths between their ciphertext and cleartext forms.
    /// </summary>
    public interface IPathConverter
    {
        /// <summary>
        /// Converts <paramref name="cleartextPath"/> into ciphertext.
        /// </summary>
        /// <param name="cleartextPath">The cleartext path to convert from.</param>
        /// <returns>A ciphertext path.</returns>
        string ToCiphertext(string cleartextPath);

        /// <summary>
        /// Converts <paramref name="ciphertextPath"/> into cleartext.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path to convert from.</param>
        /// <returns>A cleartext path.</returns>
        string ToCleartext(string ciphertextPath);

        /// <summary>
        /// Gets first cleartext filename from provided ciphertext path.
        /// </summary>
        /// <param name="ciphertextFilePath">The ciphertext path to get the filename from.</param>
        /// <returns>A cleartext filename.</returns>
        string GetCleartextFileName(string ciphertextFilePath);
    }
}
