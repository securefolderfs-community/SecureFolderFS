using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.WinUI.UserControls.InfoBars;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class FileSystemInfoBarTemplateSelector : GenericTemplateSelector<InfoBarViewModel>
    {
        public DataTemplate? DokanyUnavailableInfoBarTemplate { get; set; }

        public DataTemplate? WebDavExperimentalInfoBarTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(InfoBarViewModel? item, DependencyObject container)
        {
            if (item is DokanyInfoBar)
                return DokanyUnavailableInfoBarTemplate;

            if (item is WebDavInfoBar)
                return WebDavExperimentalInfoBarTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}
