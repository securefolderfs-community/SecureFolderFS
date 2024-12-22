using System;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents a generic model for reporting progress updates.
    /// </summary>
    /// <param name="PercentageProgress">An optional percentage progress represented by a value between 0 and 100.</param>
    /// <param name="CallbackProgress">An optional callback that reports of any failures or next progress stages.</param>
    public readonly record struct ProgressModel<T>(IProgress<double>? PercentageProgress = null, IProgress<T>? CallbackProgress = null);
}
