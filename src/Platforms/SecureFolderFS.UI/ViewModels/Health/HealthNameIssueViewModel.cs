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
        private readonly string _originalName;

        [ObservableProperty] private bool _IsEditing;
        [ObservableProperty] private string? _ItemName;

        public bool WasNameChanged => !_originalName.Equals(ItemName);

        public HealthNameIssueViewModel(IStorable storable, IResult result, string? title = null)
            : base(storable, result, title)
        {
            _originalName = storable.Name;
            ItemName = storable.Name;
            Severity = SeverityType.Warning;
        }

        partial void OnIsEditingChanged(bool value)
        {
            if (!value && string.IsNullOrWhiteSpace(ItemName))
                ItemName = _originalName;

            ErrorMessage = WasNameChanged ? "A custom name will be applied" : "Generate a new name";
        }
    }
}
