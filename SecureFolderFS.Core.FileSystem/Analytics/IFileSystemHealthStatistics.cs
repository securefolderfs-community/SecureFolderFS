namespace SecureFolderFS.Core.FileSystem.Analytics
{
    public interface IFileSystemHealthStatistics
    {
        #region Directory ID

        /// <summary>
        /// Reports that the file containing directory ID was not found.
        /// </summary>
        /// <param name="directoryIdPath">The ciphertext path to directory ID file.</param>
        void ReportDirectoryIdNotFound(string directoryIdPath);

        /// <summary>
        /// Reports that the file contents containing directory ID are invalid.
        /// </summary>
        /// <param name="directoryIdPath">The ciphertext path to directory ID file.</param>
        void ReportDirectoryIdInvalid(string directoryIdPath);

        #endregion

        #region Paths

        /// <summary>
        /// Reports that the file name could not be encrypted.
        /// </summary>
        /// <param name="cleartextFileName">The cleartext file name.</param>
        void ReportInvalidPath(string cleartextFileName);

        #endregion
    }
}
