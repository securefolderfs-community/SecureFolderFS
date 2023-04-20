using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <summary>
    /// The base class that manages UI navigation.
    /// </summary>
    public abstract partial class FrameNavigationControl : UserControl, INavigationControl
    {
        public abstract Dictionary<Type, Type> TypeBinding { get; }

        protected FrameNavigationControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public virtual Task<bool> NavigateAsync<TTarget, TTransition>(TTarget target, TTransition? transition = default)
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

        protected abstract bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (ContentFrame.Content as IDisposable)?.Dispose();
        }
    }
}
