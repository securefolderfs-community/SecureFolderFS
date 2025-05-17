using SecureFolderFS.Sdk.ViewModels.Controls.Previewers;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class PreviewerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ImageTemplate { get; set; }

        public DataTemplate? VideoTemplate { get; set; }

        public DataTemplate? TextTemplate { get; set; }

        public DataTemplate? CarouselTemplate { get; set; }
        
        public DataTemplate? FallbackTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                FallbackPreviewerViewModel => FallbackTemplate,
                CarouselPreviewerViewModel => CarouselTemplate,
                ImagePreviewerViewModel => ImageTemplate,
                VideoPreviewerViewModel => VideoTemplate,
                TextPreviewerViewModel => TextTemplate,
                _ => null
            };
        }
    }
}
