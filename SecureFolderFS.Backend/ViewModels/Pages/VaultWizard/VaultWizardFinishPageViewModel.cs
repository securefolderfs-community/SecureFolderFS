﻿using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dialogs;

namespace SecureFolderFS.Backend.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardFinishPageViewModel : BaseVaultWizardPageViewModel
    {
        public string VaultName { get; }

        public VaultWizardFinishPageViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
            this.VaultName = DialogViewModel.VaultViewModel!.VaultName;
            base.CanGoBack = false;

            DialogViewModel.PrimaryButtonEnabled = true;
            DialogViewModel.PrimaryButtonClickCommand = new RelayCommand<HandledCallback?>(_ => { }); // Override the previous action

            WeakReferenceMessenger.Default.Send(new AddVaultRequestedMessage(DialogViewModel.VaultViewModel));
        }
    }
}
