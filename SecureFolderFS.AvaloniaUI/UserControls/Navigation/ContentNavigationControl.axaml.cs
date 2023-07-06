using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <summary>
    /// The base class that manages UI navigation using <see cref="ContentControl"/>.
    /// </summary>
    // A control cannot be abstract in Avalonia
    public partial class ContentNavigationControl : UserControl, INavigationControl
    {
        protected ContentNavigationControl()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> NavigateAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default) where TTransition : class
        {
            // Get the transition finalizer which will be used to end the transition
            var transitionFinalizer = await ApplyTransitionAsync(target, transition);

            // Navigate by setting the content
            MainContent.Content = target;

            // End the transition
            if (transitionFinalizer is not null)
                await transitionFinalizer.DisposeAsync();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TTransition"></typeparam>
        /// <param name="target"></param>
        /// <param name="transition"></param>
        /// <returns></returns>
        protected virtual Task<IAsyncDisposable?> ApplyTransitionAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default) where TTransition : class
        {
            return Task.FromResult<IAsyncDisposable?>(null);
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (MainContent.Content as IDisposable)?.Dispose();
        }

        public IDataTemplate? TemplateSelector
        {
            get => GetValue(TemplateSelectorProperty);
            set => SetValue(TemplateSelectorProperty, value);
        }
        public static readonly StyledProperty<IDataTemplate?> TemplateSelectorProperty =
            AvaloniaProperty.Register<ContentNavigationControl, IDataTemplate?>(nameof(TemplateSelector));
    }
}