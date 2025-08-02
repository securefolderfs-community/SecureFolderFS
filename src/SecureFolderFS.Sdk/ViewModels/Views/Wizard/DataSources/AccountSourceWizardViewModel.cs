using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources
{
    [Bindable(true)]
    public partial class AccountSourceWizardViewModel : BaseDataSourceWizardViewModel
    {
        /// <inheritdoc/>
        public override string DataSourceName { get; }

        public AccountSourceWizardViewModel(string dataSourceName, NewVaultMode mode, IVaultCollectionModel vaultCollectionModel)
            : base(mode, vaultCollectionModel)
        {
            DataSourceName = dataSourceName;
        }

        /// <inheritdoc/>
        public override Task<IFolder?> GetFolderAsync()
        {
            return Task.FromResult<IFolder?>(null);
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }
    }
}