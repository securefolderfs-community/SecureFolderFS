using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Components
{
    [Bindable(true)]
    public abstract partial class ItemInstallationViewModel(string id, string? title) : PickerOptionViewModel(id, title), IAsyncInitialize, IProgress<double>
    {
        [ObservableProperty] private IImage? _Icon;
        [ObservableProperty] private bool _IsInstalled;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private bool _IsIndeterminate;
        [ObservableProperty] private double _CurrentProgress;
        [ObservableProperty] private string? _Description;

        /// <inheritdoc/>
        public virtual void Report(double value)
        {
            IsProgressing = true;
            CurrentProgress = value;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        [RelayCommand]
        protected abstract Task InstallAsync(CancellationToken cancellationToken);
    }
}
