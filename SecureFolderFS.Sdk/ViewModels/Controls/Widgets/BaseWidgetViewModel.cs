using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    /// <summary>
    /// A base class for all widget controls.
    /// </summary>
    public abstract class BaseWidgetViewModel : ObservableObject, IDisposable
    {
        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
