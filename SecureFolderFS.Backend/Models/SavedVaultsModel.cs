using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;

namespace SecureFolderFS.Backend.Models
{
    public sealed class SavedVaultsModel : IRecipient<AddVaultRequestedMessage>, IRecipient<RemoveVaultRequestedMessage>
    {
        public SavedVaultsModel()
        {
            WeakReferenceMessenger.Default.Register<AddVaultRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<RemoveVaultRequestedMessage>(this);
        }

        public void Receive(AddVaultRequestedMessage message)
        {
            throw new NotImplementedException();
        }

        public void Receive(RemoveVaultRequestedMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
