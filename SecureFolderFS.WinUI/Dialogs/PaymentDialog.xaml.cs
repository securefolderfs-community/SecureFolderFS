﻿using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class PaymentDialog : ContentDialog, IDialog<PaymentDialogViewModel>
    {
        /// <inheritdoc/>
        public PaymentDialogViewModel ViewModel
        {
            get => (PaymentDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public PaymentDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();
    }
}