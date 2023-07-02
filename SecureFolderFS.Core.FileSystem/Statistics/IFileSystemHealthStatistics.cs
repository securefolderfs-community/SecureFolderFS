namespace SecureFolderFS.Core.FileSystem.Statistics
{
    public interface IFileSystemHealthStatistics
    {
        #region DirectoryID

        /// <summary>
        /// Reports that the file containing DirectoryID was not found.
        /// </summary>
        /// <param name="directoryIdPath">The ciphertext path to DirectoryID file.</param>
        void ReportDirectoryIdNotFound(string directoryIdPath);

        /// <summary>
        /// Reports that the file contents containing DirectoryID are invalid.
        /// </summary>
        /// <param name="directoryIdPath">The ciphertext path to DirectoryID file.</param>
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
