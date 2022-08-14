using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy
{
    public abstract class BaseLoginStrategyViewModel : ObservableObject, IDisposable
    {
        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
