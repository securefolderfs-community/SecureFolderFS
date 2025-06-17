using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Health
{
    /// <inheritdoc cref="HealthIssueViewModel"/>
    [Bindable(true)]
    public sealed partial class HealthNameIssueViewModel : HealthIssueViewModel
    {
        [ObservableProperty] private bool _IsEditing;
        [ObservableProperty] private string? _ItemName;

        /// <summary>
        /// Gets the original name of the item before it was renamed by the user.
        /// </summary>
        public string OriginalName { get; }

        public HealthNameIssueViewModel(IStorableChild storable, IResult result, string? title = null)
            : base(storable, result, title)
        {
            OriginalName = storable.Name;
            ItemName = storable.Name;
            Severity = Severity.Warning;
        }

        partial void OnIsEditingChanged(bool value)
        {
            if (!value && string.IsNullOrWhiteSpace(ItemName))
                ItemName = OriginalName;

            var wasNameChanged = !OriginalName.Equals(ItemName);
            ErrorMessage = wasNameChanged ? "A custom name will be applied" : "Generate a new name";
        }

        [RelayCommand]
        private async Task EditAsync()
        {
            // Begin editing
            IsEditing = true;
            
            // Show rename overlay
            var newName = ItemName == OriginalName ? null : ItemName;
            var overlayViewModel = new RenameOverlayViewModel("Rename".ToLocalized()) { Message = "ChooseNewName".ToLocalized(), NewName = newName };
            await OverlayService.ShowAsync(overlayViewModel);
            
            // Update the item name and confirm the changes by switching IsEditing to false
            ItemName = overlayViewModel.NewName;
            IsEditing = false;
        }
    }
}
