using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.Utilities;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Login
{
    public abstract partial class BaseLoginViewModel : ObservableObject, INotifyStateChanged
    {
        /// <inheritdoc/>
        public abstract event EventHandler<EventArgs>? StateChanged;

        /// <summary>
        /// Sets the error to display on the view.
        /// </summary>
        /// <param name="result">The error to be set. If <paramref name="result"/> is null or <see cref="IResult.Successful"/> is true, the error is not set.</param>
        protected abstract void SetError(IResult? result);
    }
}
