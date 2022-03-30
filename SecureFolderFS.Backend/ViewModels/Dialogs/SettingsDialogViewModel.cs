using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dialogs
{
    public sealed class SettingsDialogViewModel : ObservableObject
    {
        public IMessenger Messenger { get; }

        public SettingsDialogViewModel()
        {
            this.Messenger = new WeakReferenceMessenger();
        }
    }
}
