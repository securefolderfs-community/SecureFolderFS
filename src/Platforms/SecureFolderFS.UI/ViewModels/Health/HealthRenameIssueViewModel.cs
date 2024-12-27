using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Health
{
    /// <inheritdoc cref="HealthIssueViewModel"/>
    public sealed partial class HealthRenameIssueViewModel : HealthIssueViewModel
    {
        [ObservableProperty] private bool _IsEditing;
        [ObservableProperty] private string? _ItemName;

        public HealthRenameIssueViewModel(IStorable storable, IResult result, string? title = null)
            : base(storable, result, title)
        {
            ItemName = storable.Name;
        }
    }
}
