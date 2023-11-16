using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class ChangelogDialog : ContentDialog, IDialog<ChangelogDialogViewModel>
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        /// <inheritdoc/>
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
        public new async Task<IResult> ShowAsync() => DialogExtensions.ResultFromDialogOption((DialogOption)await base.ShowAsync());

        //private async void MarkdownTextBlock_LinkClicked(object? sender, LinkClickedEventArgs e)
        //{
        //    var uri = new Uri(e.Link);
        //    await ApplicationService.OpenUriAsync(uri);
        //}
    }
}
