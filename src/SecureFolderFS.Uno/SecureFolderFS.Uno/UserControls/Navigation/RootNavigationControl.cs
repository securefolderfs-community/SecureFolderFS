using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Uno.UserControls.Navigation
{
    /// <inheritdoc cref="ContentNavigationControl"/>
    internal sealed partial class RootNavigationControl : ContentNavigationControl
    {
        /// <inheritdoc/>
        protected override Task<IAsyncDisposable?> ApplyTransitionAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default) where TTarget : default where TTransition : class
        {
            // Use transitions only when the initial page view model is not MainAppHostViewModel 
            if ((MainContent.Content is null && target is not MainHostViewModel)
                || (MainContent.Content is not MainHostViewModel &&
                    MainContent.Content is not null && target is MainHostViewModel))
            {
                MainContent.ContentTransitions.Clear();
                MainContent.ContentTransitions.Add(new ContentThemeTransition());

                return Task.FromResult<IAsyncDisposable?>(new RootContentTransition(MainContent.ContentTransitions));
            }

            return Task.FromResult<IAsyncDisposable?>(null);
        }
    }

    file sealed class RootContentTransition : IAsyncDisposable
    {
        private readonly TransitionCollection _transitionCollection;

        public RootContentTransition(TransitionCollection transitionCollection)
        {
            _transitionCollection = transitionCollection;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await Task.Delay(250);
            _transitionCollection.Clear();
        }
    }
}
