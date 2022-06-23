using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.Models
{
    public interface INavigationModel
    {
        void ResetNavigation();

        IEquatable<INotifyPropertyChanged> PopLastViewModel();
    }
}
