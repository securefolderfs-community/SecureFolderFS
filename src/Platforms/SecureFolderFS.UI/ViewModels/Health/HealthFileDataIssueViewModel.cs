using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Health
{
    /// <inheritdoc cref="HealthIssueViewModel"/>
    [Bindable(true)]
    public sealed partial class HealthFileDataIssueViewModel : HealthIssueViewModel
    {
        /// <summary>
        /// Gets or sets whether this file can be repaired (chunks can be zeroed out).
        /// </summary>
        [ObservableProperty] private bool _IsRecoverable;
        
        /// <summary>
        /// Gets or sets the text representation of corrupted chunks.
        /// </summary>
        [ObservableProperty] private string? _CorruptedChunksText;
        
        public IFile? File => Inner as IFile;

        /// <summary>
        /// Gets the list of corrupted chunk numbers in the file.
        /// </summary>
        public IReadOnlyList<long> CorruptedChunks { get; }

        public HealthFileDataIssueViewModel(IStorableChild storable, IResult? result, string? title = null, IReadOnlyList<long>? corruptedChunks = null, bool isRecoverable = true)
            : base(storable, result, title)
        {
            Severity = Severity.Critical;
            CorruptedChunks = corruptedChunks ?? [];
            CorruptedChunksText = !isRecoverable ? string.Empty : "CorruptedChunks".ToLocalized(CorruptedChunks.Count);
            IsRecoverable = isRecoverable;
        }
    }
}
