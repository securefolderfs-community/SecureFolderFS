using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Components
{
    [Bindable(true)]
    public sealed partial class BreadcrumbItemViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsLeading;
        [ObservableProperty] private ICommand? _Command;

        public BreadcrumbItemViewModel(string? title, ICommand? command)
        {
            Title = title;
            Command = command;
            IsLeading = true;
        }
    }
}
