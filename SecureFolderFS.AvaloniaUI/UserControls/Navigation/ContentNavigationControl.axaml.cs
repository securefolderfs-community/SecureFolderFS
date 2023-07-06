using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <summary>
    /// The base class that manages UI navigation using <see cref="ContentControl"/>.
    /// </summary>
    public abstract partial class ContentNavigationControl : UserControl, INavigationControl
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
        protected abstract Task<IAsyncDisposable?> ApplyTransitionAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default) where TTransition : class;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (MainContent.Content as IDisposable)?.Dispose();
        }

        public DataTemplate? TemplateSelector
        {
            get => (DataTemplate?)GetValue(TemplateSelectorProperty);
            set => SetValue(TemplateSelectorProperty, value);
        }
        public static readonly StyledProperty<DataTemplate?> TemplateSelectorProperty =
            AvaloniaProperty.Register<ContentNavigationControl, DataTemplate?>(nameof(TemplateSelector));
    }
}