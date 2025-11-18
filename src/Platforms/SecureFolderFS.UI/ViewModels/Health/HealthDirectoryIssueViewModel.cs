using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Health
{
    /// <inheritdoc cref="HealthIssueViewModel"/>
    [Bindable(true)]
    public sealed partial class HealthDirectoryIssueViewModel : HealthIssueViewModel, IAsyncInitialize
    {
        [ObservableProperty] private ObservableCollection<HealthIssueViewModel> _Issues;

        private IVaultService VaultService { get; } = DI.Service<IVaultService>();

        public IFolder? Folder => Inner as IFolder;

        public HealthDirectoryIssueViewModel(IStorableChild storable, IResult result, string? title = null)
            : base(storable, result, title)
        {
            Severity = Severity.Warning;
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
                if (VaultService.IsNameReserved(item.Name))
                    continue;

                Issues.Add(new HealthNameIssueViewModel(item, Shared.Models.Result.Failure(null), "InvalidItemName".ToLocalized())
                {
                    ErrorMessage = "GenerateNewName".ToLocalized()
                });
            }
        }
    }
}
