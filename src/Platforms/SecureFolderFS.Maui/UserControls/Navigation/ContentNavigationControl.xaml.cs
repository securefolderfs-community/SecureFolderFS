using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Maui.UserControls.Navigation
{
    public abstract partial class ContentNavigationControl : ContentView, INavigator, IDisposable
    {
        public ContentNavigationControl()
        {
            InitializeComponent();
            RootGrid.BindingContext = this;
        }

        /// <inheritdoc/>
        public async Task<bool> NavigateAsync(IViewDesignation? view)
        {
            // Get the transition finalizer which will be used to end the transition
            var transitionFinalizer = await ApplyTransitionAsync(view);

            // Navigate by setting the content
            Presentation.Presentation = view;

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
        public void Dispose()
        {
            (Presentation.Presentation as IDisposable)?.Dispose();
        }

        public DataTemplateSelector? TemplateSelector
        {
            get => (DataTemplateSelector?)GetValue(TemplateSelectorProperty);
            set => SetValue(TemplateSelectorProperty, value);
        }
        public static readonly BindableProperty TemplateSelectorProperty =
            BindableProperty.Create(nameof(TemplateSelector), typeof(DataTemplateSelector), typeof(ContentNavigationControl), null);
    }
}
