using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Bindable(true)]
    public abstract partial class ReportableViewModel : ObservableObject, IViewable, IProgress<IResult?>, INotifyStateChanged
    {
        /// <inheritdoc cref="IViewable.Title"/>
        [ObservableProperty] private string? _Title;

        /// <inheritdoc/>
        public abstract event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public abstract void Report(IResult? value);
    }
}
