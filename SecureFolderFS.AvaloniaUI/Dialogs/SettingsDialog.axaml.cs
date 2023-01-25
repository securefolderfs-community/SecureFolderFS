using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    public partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}