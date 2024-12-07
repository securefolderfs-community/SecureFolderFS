using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Bindable(true)]
    public sealed partial class ErrorViewModel : ReportableViewModel
    {
        [ObservableProperty] private string? _Message;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public ErrorViewModel(string? title, string? message)
        {
            Title = title;
            Message = message;
        }

        /// <inheritdoc/>
        public override void Report(IResult? result)
        {
            Message = result?.GetMessage();
        }
    }
}
