using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.Models
{
    public interface INavigationModel
    {
        void ResetNavigation();

        void ResetNavigation<TViewModel>(TViewModel viewModel) where TViewModel : class, INotifyPropertyChanged, IEquatable<TViewModel>;
    }
}
