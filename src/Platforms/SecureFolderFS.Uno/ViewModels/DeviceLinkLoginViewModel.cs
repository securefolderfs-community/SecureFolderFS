using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.UI.DataModels;

namespace SecureFolderFS.Uno.ViewModels
{
    [Bindable(true)]
    public sealed partial class DeviceLinkLoginViewModel : DeviceLinkViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;
        
        public DeviceLinkLoginViewModel(IFolder vaultFolder, string vaultId)
            : base(vaultFolder, vaultId)
        {
        }

        protected override Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task<DeviceLinkVaultDataModel> GetDataModelAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
