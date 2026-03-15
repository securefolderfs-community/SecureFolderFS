namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents a countable sequence of elements.
    /// </summary>
    public interface ILengthMeasurable
    {
        /// <summary>
        /// Gets the number of elements in the countable sequence.
        /// </summary>
        int Length { get; }
    }
}
