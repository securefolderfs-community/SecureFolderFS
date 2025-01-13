using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Health
{
    /// <inheritdoc cref="HealthIssueViewModel"/>
    public sealed partial class HealthNameIssueViewModel : HealthIssueViewModel
    {
        [ObservableProperty] private bool _IsEditing;
        [ObservableProperty] private string? _ItemName;

        public string OriginalName { get; }

        public HealthNameIssueViewModel(IStorableChild storable, IResult result, string? title = null)
            : base(storable, result, title)
        {
            OriginalName = storable.Name;
            ItemName = storable.Name;
            Severity = SeverityType.Warning;
        }

        partial void OnIsEditingChanged(bool value)
        {
            if (!value && string.IsNullOrWhiteSpace(ItemName))
                ItemName = OriginalName;

            var wasNameChanged = !OriginalName.Equals(ItemName);
            ErrorMessage = wasNameChanged ? "A custom name will be applied" : "Generate a new name";
        }
    }
}
