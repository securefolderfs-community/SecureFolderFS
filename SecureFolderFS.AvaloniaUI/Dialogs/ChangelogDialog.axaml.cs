using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    public sealed partial class ChangelogDialog : ContentDialog, IStyleable, IDialog<ChangelogDialogViewModel>
    {
        /// <inheritdoc/>
        public ChangelogDialogViewModel ViewModel
        {
            get => (ChangelogDialogViewModel)DataContext;
            set => DataContext = value;
        }
        
        public ChangelogDialog()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public Type StyleKey => typeof(ContentDialog);

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync() => DialogExtensions.ResultFromDialogOption((DialogOption)await base.ShowAsync());
    }
}