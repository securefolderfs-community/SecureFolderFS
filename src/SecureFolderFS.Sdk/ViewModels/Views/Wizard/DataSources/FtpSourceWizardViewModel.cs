using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources
{
    [Bindable(true)]
    public sealed partial class FtpSourceWizardViewModel : BaseDataSourceWizardViewModel
    {
        /// <inheritdoc/>
        public override string DataSourceName { get; } = "FTP";

        public FtpSourceWizardViewModel(NewVaultMode mode, IVaultCollectionModel vaultCollectionModel)
            : base(mode, vaultCollectionModel)
        {
        }

        /// <inheritdoc/>
        public override Task<IFolder?> GetFolderAsync()
        {
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }
    }
}