using System;
using System.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Bindable(true)]
    public partial class UnsupportedViewModel : ReportableViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public UnsupportedViewModel(string? title)
        {
            Title = title;
        }

        /// <inheritdoc/>
        public override void Report(IResult? value)
        {
            _ = value;
        }
    }
}
