namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents the current progress stage and total progress.
    /// </summary>
    /// <param name="Achieved">The current achieved progress so far.</param>
    /// <param name="Total">The total progress to achieve.</param>
    /// <param name="State">The additional state of the progress.</param>
    public readonly record struct TotalProgress(int Achieved, int Total, object? State);
}
