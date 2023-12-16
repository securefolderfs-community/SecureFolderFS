namespace SecureFolderFS.Core.FileSystem.Paths
{
    /// <summary>
    /// Manages and converts paths between their ciphertext and cleartext forms.
    /// </summary>
    public interface IPathConverter
    {
        // TODO: Use ReadOnlySpan<char> for all methods

        /// <summary>
        /// Converts <paramref name="cleartextPath"/> into ciphertext.
        /// </summary>
        /// <param name="cleartextPath">The relative cleartext path to convert from.</param>
        /// <returns>If successful, value represents a ciphertext path; otherwise null.</returns>
        string? ToCiphertext(string cleartextPath);

        /// <summary>
        /// Converts <paramref name="ciphertextPath"/> into cleartext.
        /// </summary>
        /// <param name="ciphertextPath">The relative ciphertext path to convert from.</param>
        /// <returns>If successful, value represents a cleartext path; otherwise null.</returns>
        string? ToCleartext(string ciphertextPath);

        /// <summary>
        /// Gets first cleartext filename from provided ciphertext path.
        /// </summary>
        /// <param name="ciphertextFilePath">The ciphertext path to get the filename from.</param>
        /// <returns>If successful, value represents a cleartext filename; otherwise null.</returns>
        string? GetCleartextFileName(string ciphertextFilePath);

        /// <summary>
        /// Gets first ciphertext filename from provided cleartext path.
        /// </summary>
        /// <param name="cleartextFilePath">The cleartext path to get the filename from.</param>
        /// <returns>If successful, value represents a ciphertext filename; otherwise null.</returns>
        string? GetCiphertextFileName(string cleartextFilePath);
    }
}
