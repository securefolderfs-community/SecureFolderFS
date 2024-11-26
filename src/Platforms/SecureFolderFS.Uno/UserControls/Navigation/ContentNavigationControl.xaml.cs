using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Navigation
{
    /// <summary>
    /// The base class that manages UI navigation using <see cref="ContentControl"/>.
    /// </summary>
    public abstract partial class ContentNavigationControl : UserControl, INavigator, IDisposable
    {
        protected ContentNavigationControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<bool> NavigateAsync(IViewDesignation? view)
        {
            // Get the transition finalizer which will be used to end the transition
            var transitionFinalizer = await ApplyTransitionAsync(view);

            // Navigate by setting the content
            MainContent.Content = view;

            // End the transition
            if (transitionFinalizer is not null)
                await transitionFinalizer.DisposeAsync();

            return true;
        }

        /// <inheritdoc/>
        public Task<bool> GoBackAsync()
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> GoForwardAsync()
        {
            return Task.FromResult(false);
        }

        protected abstract Task<IAsyncDisposable?> ApplyTransitionAsync<TTarget>(TTarget? target);

        /// <inheritdoc/>
        public new void Dispose()
        {
            (MainContent.Content as IDisposable)?.Dispose();
        }

        public DataTemplateSelector? TemplateSelector
        {
            get => (DataTemplateSelector?)GetValue(TemplateSelectorProperty);
            set => SetValue(TemplateSelectorProperty, value);
        }
        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.Register(nameof(TemplateSelector), typeof(DataTemplateSelector), typeof(ContentNavigationControl), new PropertyMetadata(defaultValue: null));
    }
}
