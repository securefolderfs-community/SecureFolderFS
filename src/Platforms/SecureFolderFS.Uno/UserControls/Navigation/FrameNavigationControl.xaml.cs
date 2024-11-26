using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Navigation
{
    /// <summary>
    /// The base class that manages UI navigation using <see cref="Frame"/>.
    /// </summary>
    public abstract partial class FrameNavigationControl : UserControl, INavigator, IDisposable
    {
        /// <summary>
        /// Gets a dictionary of types that bind view models and pages together.
        /// </summary>
        public abstract Dictionary<Type, Type> TypeBinding { get; }

        protected FrameNavigationControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public Task<bool> NavigateAsync(IViewDesignation? view)
        {
            if (view is null)
            {
                ContentFrame.Content = null;
                return Task.FromResult(true);
            }

            var pageType = TypeBinding.GetByKeyOrValue(view.GetType());
            if (pageType is null)
                return Task.FromResult(false);

            var result = NavigateFrame(pageType, view);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<bool> GoBackAsync()
        {
            if (!ContentFrame.CanGoBack)
                return Task.FromResult(false);
                
            ContentFrame.GoBack();
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> GoForwardAsync()
        {
            if (!ContentFrame.CanGoForward)
                return Task.FromResult(false);
                
            ContentFrame.GoForward();
            return Task.FromResult(true);
        }

        /// <summary>
        /// Resets the current content of the <see cref="ContentFrame"/>.
        /// </summary>
        public virtual void ClearContent()
        {
            ContentFrame.Content = null;
        }

        /// <summary>
        /// Navigates a frame to specified <paramref name="pageType"/>.
        /// </summary>
        /// <param name="pageType">The type of page to navigate to.</param>
        /// <param name="parameter">The parameter to pass to the page.</param>
        /// <returns>If successful, returns true; otherwise false.</returns>
        protected abstract bool NavigateFrame(Type pageType, object parameter);

        /// <inheritdoc/>
        public new void Dispose()
        {
            ClearContent();
            (ContentFrame.Content as IDisposable)?.Dispose();
        }
    }
}
