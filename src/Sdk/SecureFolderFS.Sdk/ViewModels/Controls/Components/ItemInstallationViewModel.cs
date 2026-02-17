using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Components
{
    [Bindable(true)]
    public abstract partial class ItemInstallationViewModel(string id, string? title) : PickerOptionViewModel(id, title), IProgress<double>
    {
        [ObservableProperty] private bool _IsInstalled;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private double _CurrentProgress;
        [ObservableProperty] private string? _Description;

        /// <inheritdoc/>
        public virtual void Report(double value)
        {
            IsProgressing = true;
            CurrentProgress = value;
        }

        [RelayCommand]
        protected abstract Task InstallAsync(CancellationToken cancellationToken);
    }
}
