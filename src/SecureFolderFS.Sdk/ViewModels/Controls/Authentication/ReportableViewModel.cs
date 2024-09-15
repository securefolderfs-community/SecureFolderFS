using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Bindable(true)]
    public abstract class ReportableViewModel : ObservableObject, IProgress<IResult?>, INotifyStateChanged
    {
        /// <inheritdoc/>
        public abstract event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public abstract void Report(IResult? value);
    }
}
