namespace SecureFolderFS.Core.Sdk.Paths
{
    /// <summary>
    /// Provides module for handling SecureFolderFS paths.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IPathReceiver
    {
        /// <summary>
        /// Converts provided <paramref name="cleartextPath"/> into ciphertext path.
        /// </summary>
        /// <param name="cleartextPath">The cleartext path to convert from.</param>
        /// <returns>A ciphertext path.</returns>
        string ToCiphertext(string cleartextPath);

        /// <summary>
        /// Converts provided <paramref name="ciphertextPath"/> into cleartext path.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path to convert from.</param>
        /// <returns>A cleartext path.</returns>
        string ToCleartext(string ciphertextPath);

        /// <summary>
        /// Gets active cleartext file name from provided ciphertext path.
        /// </summary>
        /// <param name="ciphertextFilePath">The path.</param>
        /// <returns></returns>
        string GetCleartextFileName(string ciphertextFilePath);

        /// <summary>
        /// Gets active ciphertext file name from provided cleartext path.
        /// </summary>
        /// <param name="cleartextFilePath">The path.</param>
        /// <returns></returns>
        string GetCiphertextFileName(string cleartextFilePath);
    }
}
