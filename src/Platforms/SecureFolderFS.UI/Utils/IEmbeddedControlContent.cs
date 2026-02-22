namespace SecureFolderFS.UI.Utils
{
    /// <summary>
    /// Allows a control to expose embedded content.
    /// </summary>
    public interface IEmbeddedControlContent
    {
        /// <summary>
        /// Gets the embedded content associated with the control.
        /// </summary>
        object? EmbeddedContent { get; }
    }
}
