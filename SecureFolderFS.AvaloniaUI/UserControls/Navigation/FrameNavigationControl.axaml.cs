using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <summary>
    /// The base class that manages UI navigation.
    /// </summary>
    public partial class FrameNavigationControl : UserControl, INavigationControl
    {
        /// <summary>
        /// Gets a dictionary of types that bind view models and pages together.
        /// </summary>
        public virtual Dictionary<Type, Type> TypeBinding { get; }

        public FrameNavigationControl()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> NavigateAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default)
            where TTransition : class
        {
            if (target is null)
            {
                ContentFrame.Content = null;
                return true;
            }

            var pageType = TypeBinding.GetByKeyOrValue(target.GetType());
            if (pageType is null)
                return false;

            (ContentFrame.Content as Page)?.OnNavigatingFrom();
            await Task.Delay(25);

            var result = NavigateFrame(pageType, target, transition as NavigationTransitionInfo);
            (ContentFrame.Content as Page)?.OnNavigatedTo(new(null, NavigationMode.New, null, target, null));
            return result;
        }

        /// <summary>
        /// Navigates a frame to specified <paramref name="pageType"/>.
        /// </summary>
        /// <param name="pageType">The type of page to navigate to.</param>
        /// <param name="parameter">The parameter to pass to the page.</param>
        /// <param name="transition">The transition to use when navigating.</param>
        /// <returns>If successful, returns true, otherwise false.</returns>
        protected virtual bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo)
        {
            return false;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (ContentFrame.Content as IDisposable)?.Dispose();
        }
    }
}