using System.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Health
{
    /// <inheritdoc cref="HealthIssueViewModel"/>
    [Bindable(true)]
    public sealed class HealthNormalizationIssueViewModel : HealthIssueViewModel
    {
        public HealthNormalizationIssueViewModel(IStorableChild storable, IResult result, string? title = null)
            : base(storable, result, title)
        {
            Severity = Severity.Warning;
        }
    }
}
