using SecureFolderFS.Sdk.ViewModels.Controls.Previewers;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class PreviewerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ImageTemplate { get; set; }

        public DataTemplate? VideoTemplate { get; set; }

        public DataTemplate? AudioTemplate { get; set; }

        public DataTemplate? TextTemplate { get; set; }

        public DataTemplate? PdfTemplate { get; set; }

        public DataTemplate? ArchiveTemplate { get; set; }

        public DataTemplate? CarouselTemplate { get; set; }

        public DataTemplate? FallbackTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                FallbackPreviewerViewModel => FallbackTemplate,
                CarouselPreviewerViewModel => CarouselTemplate,
                ArchivePreviewerViewModel => ArchiveTemplate,
                ImagePreviewerViewModel => ImageTemplate,
                VideoPreviewerViewModel => VideoTemplate,
                AudioPreviewerViewModel => AudioTemplate,
                TextPreviewerViewModel => TextTemplate,
                PdfPreviewerViewModel => PdfTemplate,
                _ => null
            };
        }
    }
}
