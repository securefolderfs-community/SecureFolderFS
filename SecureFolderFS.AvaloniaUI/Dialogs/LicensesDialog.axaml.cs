using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.AvaloniaUI.Messages;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    internal sealed partial class LicensesDialog : ContentDialog, IDialog<LicensesDialogViewModel>, IStyleable
    {
        /// <inheritdoc/>
        public LicensesDialogViewModel ViewModel
        {
            get => (LicensesDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public LicensesDialog()
        {
            InitializeComponent();
        }

        public Type StyleKey => typeof(ContentDialog);

        /// <inheritdoc/>
        public async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            Hide();
            WeakReferenceMessenger.Default.Send(new DialogHiddenMessage());
        }

        private void Control_OnLoaded(object? sender, RoutedEventArgs e)
        {
            _ = ViewModel.InitAsync();
        }

        private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}