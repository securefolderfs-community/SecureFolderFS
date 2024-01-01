using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    /// <summary>
    /// A base class for all widget view models.
    /// </summary>
    public abstract class BaseWidgetViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        public IWidgetModel WidgetModel { get; }

        protected BaseWidgetViewModel(IWidgetModel widgetModel)
        {
            WidgetModel = widgetModel;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void Dispose() { }
    }
}
