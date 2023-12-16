using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Navigation
{
    /// <summary>
    /// The base class that manages UI navigation using <see cref="Frame"/>.
    /// </summary>
    public abstract partial class FrameNavigationControl : UserControl, INavigationControl
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
        public virtual Task<bool> NavigateAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default)
            where TTransition : class
        {
            if (target is null)
            {
                ContentFrame.Content = null;
                return Task.FromResult(true);
            }

            var pageType = TypeBinding.GetByKeyOrValue(target.GetType());
            if (pageType is null)
                return Task.FromResult(false);

            var result = NavigateFrame(pageType, target, transition as NavigationTransitionInfo);
            return Task.FromResult(result);
        }

        /// <summary>
        /// Navigates a frame to specified <paramref name="pageType"/>.
        /// </summary>
        /// <param name="pageType">The type of page to navigate to.</param>
        /// <param name="parameter">The parameter to pass to the page.</param>
        /// <param name="transitionInfo">The transition to use when navigating.</param>
        /// <returns>If successful, returns true; otherwise false.</returns>
        protected abstract bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (ContentFrame.Content as IDisposable)?.Dispose();
        }
    }
}
