using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class ChangelogDialog : ContentDialog, IOverlayControl
    {
        private IApplicationService ApplicationService { get; } = DI.Service<IApplicationService>();

        public ChangelogOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<ChangelogOverlayViewModel>();
            set => DataContext = value;
        }

        public ChangelogDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => (await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (ChangelogOverlayViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        //private async void MarkdownTextBlock_LinkClicked(object? sender, LinkClickedEventArgs e)
        //{
        //    var uri = new Uri(e.Link);
        //    await ApplicationService.OpenUriAsync(uri);
        //}
    }
}
