using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.EventArguments;

namespace SecureFolderFS.Uno.ViewModels.DeviceLink
{
    [Bindable(true)]
    public sealed partial class DeviceLinkLoginViewModel(IFolder vaultFolder, string vaultId)
        : DeviceLinkViewModel(vaultFolder, vaultId)
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;
        
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;
        
        /// <inheritdoc/>
        protected override Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Task<bool> ShowVerificationCodeAsync(string code)
        {
            throw new NotSupportedException();
        }
    }
}
