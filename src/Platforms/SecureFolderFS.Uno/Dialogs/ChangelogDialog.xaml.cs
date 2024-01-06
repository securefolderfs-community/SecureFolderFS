using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class ChangelogDialog : ContentDialog, IOverlayControl
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        public ChangelogDialogViewModel ViewModel
        {
            get => (ChangelogDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public ChangelogDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => ((DialogOption)await base.ShowAsync()).ResultFromDialogOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (ChangelogDialogViewModel)viewable;

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
