using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views
{
    public abstract partial class ReportableViewModel : ObservableObject, INotifyStateChanged
    {
        /// <inheritdoc/>
        public abstract event EventHandler<EventArgs>? StateChanged;

        /// <summary>
        /// Sets the error to display on the view.
        /// </summary>
        /// <param name="result">The error to be set. If <paramref name="result"/> is null or <see cref="IResult.Successful"/> is true, the error is unset.</param>
        public abstract void SetError(IResult? result);
    }
}
