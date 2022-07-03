using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    /// <summary>
    /// A base class for all widget controls.
    /// </summary>
    public abstract class BaseWidgetViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
