using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.AvaloniaUI.Messages;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    public sealed partial class LicensesDialog : ContentDialog, IDialog<LicensesDialogViewModel>, IStyleable
    {
        /// <inheritdoc/>
        public LicensesDialogViewModel? ViewModel
        {
            get => (LicensesDialogViewModel?)DataContext;
            set => DataContext = value;
        }

        public LicensesDialog()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Type StyleKey => typeof(ContentDialog);

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => DialogExtensions.ResultFromDialogOption((DialogOption)await base.ShowAsync());

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Hide();
            WeakReferenceMessenger.Default.Send(new DialogHiddenMessage());
        }

        private void Licenses_Loaded(object? sender, RoutedEventArgs e)
        {
            _ = ViewModel.InitAsync();
        }
    }
}