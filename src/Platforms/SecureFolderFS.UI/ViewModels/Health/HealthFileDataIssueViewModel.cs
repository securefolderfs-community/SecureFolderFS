using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Health
{
    /// <inheritdoc cref="HealthIssueViewModel"/>

    public sealed partial class HealthFileDataIssueViewModel : HealthIssueViewModel
    {
        public IFile? File => Inner as IFile;

        public HealthFileDataIssueViewModel(IStorable storable, IResult? result, string? title = null)
            : base(storable, result, title)
        {
            Severity = SeverityType.Critical;
        }
    }
}
