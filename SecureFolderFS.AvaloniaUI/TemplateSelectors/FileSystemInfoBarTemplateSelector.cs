using Avalonia.Markup.Xaml.Templates;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.UI.UserControls.InfoBars;

namespace SecureFolderFS.AvaloniaUI.TemplateSelectors
{
    internal sealed class FileSystemInfoBarTemplateSelector : GenericTemplateSelector<InfoBarViewModel>
    {
        public DataTemplate? DokanyUnavailableInfoBarTemplate { get; set; }

        public DataTemplate? FuseExperimentalInfoBarTemplate { get; set; }

        public DataTemplate? WebDavExperimentalInfoBarTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(InfoBarViewModel? item)
        {
            return item switch
            {
                DokanyInfoBar => DokanyUnavailableInfoBarTemplate,
                FuseInfoBar => FuseExperimentalInfoBarTemplate,
                WebDavInfoBar => WebDavExperimentalInfoBarTemplate,
                _ => null
            };
        }
    }
}