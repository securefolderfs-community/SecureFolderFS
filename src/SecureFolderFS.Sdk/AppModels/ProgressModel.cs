using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents a generic model for reporting progress updates.
    /// </summary>
    /// <param name="PrecisionProgress">Precise progress represented by a value between 0 and 100.</param>
    /// <param name="CallbackProgress">An optional callback that reports of any failures or next progress stages.</param>
    public record class ProgressModel(IProgress<float>? PrecisionProgress = null, IProgress<IResult>? CallbackProgress = null);
}
