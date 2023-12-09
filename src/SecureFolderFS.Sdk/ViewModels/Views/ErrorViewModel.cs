using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views
{
    public sealed partial class ErrorViewModel : ReportableViewModel
    {
        [ObservableProperty] private string? _ErrorMessage;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public ErrorViewModel(string errorMessage)
        {
            _ErrorMessage = errorMessage;
        }

        /// <inheritdoc/>
        protected override void SetError(IResult? result)
        {
            ErrorMessage = result?.GetMessage();
        }
    }
}
