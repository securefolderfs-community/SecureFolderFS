using System.Collections.Generic;
using System.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Health
{
    /// <inheritdoc cref="HealthIssueViewModel"/>
    [Bindable(true)]
    public sealed partial class HealthFileDataIssueViewModel : HealthIssueViewModel
    {
        public IFile? File => Inner as IFile;

        /// <summary>
        /// Gets the list of corrupted chunk numbers in the file.
        /// </summary>
        public IReadOnlyList<long> CorruptedChunks { get; }

        /// <summary>
        /// Gets whether this file can be repaired (chunks can be zeroed out).
        /// </summary>
        public bool IsRecoverable { get; }

        public HealthFileDataIssueViewModel(IStorableChild storable, IResult? result, string? title = null, IReadOnlyList<long>? corruptedChunks = null, bool isRecoverable = true)
            : base(storable, result, title)
        {
            Severity = Severity.Critical;
            CorruptedChunks = corruptedChunks ?? System.Array.Empty<long>();
            IsRecoverable = isRecoverable;
        }
    }
}
