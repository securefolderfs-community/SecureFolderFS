using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.Models
{
    public interface INavigationModel // TODO: Still used?
    {
        void ResetNavigation();

        IEquatable<INotifyPropertyChanged> PopLastViewModel();
    }
}
