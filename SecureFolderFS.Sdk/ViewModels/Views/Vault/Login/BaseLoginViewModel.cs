using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Shared.Utilities;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Login
{
    public abstract partial class BaseLoginViewModel : ObservableObject, INotifyStateChanged
    {
        protected readonly AuthenticationModel authenticationModel;

        /// <inheritdoc/>
        public event EventHandler<EventArgs>? StateChanged;

        protected BaseLoginViewModel(AuthenticationModel authenticationModel)
        {
            this.authenticationModel = authenticationModel;
        }

        protected abstract void SetError(IResult? result);

        protected void InvokeStateChanged(object sender, EventArgs e)
        {
            StateChanged?.Invoke(sender, e);
        }
    }
}
