using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    /// <summary>
    /// A base class for all widget view models.
    /// </summary>
    [Bindable(true)]
    public abstract class BaseWidgetViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        /// <summary>
        /// Gets the widget model interface used for manipulating widget data.
        /// </summary>
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
