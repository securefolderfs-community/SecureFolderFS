using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.UserControls.Navigation
{
    public abstract partial class ContentNavigationControl : ContentView, INavigationControl
    {
        public ContentNavigationControl()
        {
            InitializeComponent();
            RootGrid.BindingContext = this;
        }

        /// <inheritdoc/>
        public async Task<bool> NavigateAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default)
            where TTarget : IViewDesignation
            where TTransition : class
        {
            // Get the transition finalizer which will be used to end the transition
            var transitionFinalizer = await ApplyTransitionAsync(target, transition);

            // Navigate by setting the content
            Presentation.ViewContent = target;

            // End the transition
            if (transitionFinalizer is not null)
                await transitionFinalizer.DisposeAsync();

            return true;
        }

        protected abstract Task<IAsyncDisposable?> ApplyTransitionAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default) where TTransition : class;

        /// <inheritdoc/>
        public void Dispose()
        {
            (Presentation.ViewContent as IDisposable)?.Dispose();
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
