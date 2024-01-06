using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views
{
    /// <summary>
    /// Represents a base view model for pages which allows for navigation.
    /// </summary>
    public abstract partial class BasePageViewModel : ObservableObject, INavigationTarget, IAsyncInitialize, IViewable
    {
        /// <inheritdoc cref="IViewable.Title"/>
        [ObservableProperty] private string? _Title;

        /// <inheritdoc/>
        public virtual void OnNavigatingTo(NavigationType navigationType)
        {
        }

        /// <inheritdoc/>
        public virtual void OnNavigatingFrom()
        {
        }

        /// <inheritdoc/>
        public virtual Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
