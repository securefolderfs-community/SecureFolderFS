using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views
{
    /// <summary>
    /// Represents a base view model for pages which allows for navigation.
    /// </summary>
    public abstract class BasePageViewModel : ObservableObject, INavigationTarget, IAsyncInitialize
    {
        /// <inheritdoc/>
        public void OnNavigatingTo(NavigationType navigationType)
        {
        }

        /// <inheritdoc/>
        public void OnNavigatingFrom()
        {
        }

        /// <inheritdoc/>
        public virtual Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
