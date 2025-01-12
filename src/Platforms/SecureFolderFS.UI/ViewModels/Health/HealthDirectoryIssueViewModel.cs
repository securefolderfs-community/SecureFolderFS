using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Health
{
    /// <inheritdoc cref="HealthIssueViewModel"/>
    public sealed partial class HealthDirectoryIssueViewModel : HealthIssueViewModel, IAsyncInitialize
    {
        [ObservableProperty] private ObservableCollection<HealthIssueViewModel> _Issues;

        public IFolder? Folder => Inner as IFolder;

        public HealthDirectoryIssueViewModel(IStorable storable, IResult result, string? title = null)
            : base(storable, result, title)
        {
            Severity = SeverityType.Warning;
            Issues = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (Folder is null)
                return;

            Issues.Clear();
            await foreach (var item in Folder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                Issues.Add(new HealthNameIssueViewModel(item, Shared.Models.Result.Failure(null), "Invalid name")
                {
                    ErrorMessage = "Generate a new name"
                });
            }
        }
    }
}
