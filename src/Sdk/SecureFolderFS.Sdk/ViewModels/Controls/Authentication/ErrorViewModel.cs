using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Bindable(true)]
    public partial class ErrorViewModel : ReportableViewModel
    {
        /// <summary>
        /// Gets the error message of the <see cref="IResult"/>. If no errors are reported, value is null.
        /// </summary>
        [ObservableProperty] private string? _ErrorMessage;

        /// <summary>
        /// Gets the exception message of the <see cref="IResult"/>. If no errors are reported, value is null.
        /// </summary>
        [ObservableProperty] private string? _ExceptionMessage;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public ErrorViewModel(string? title)
        {
            Title = title;
        }

        public ErrorViewModel(IResult? error)
        {
            Report(error);
        }

        /// <inheritdoc/>
        public sealed override void Report(IResult? result)
        {
            UpdateStatus(result);
        }

        /// <summary>
        /// Updates the current state upon an error report.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> that was reported.</param>
        protected virtual void UpdateStatus(IResult? result)
        {
            if (result is { Successful: false })
            {
                ErrorMessage = result.GetMessage("UnknownError".ToLocalized());
                ExceptionMessage = result.GetExceptionMessage("UnknownError".ToLocalized());
            }
            else
            {
                ErrorMessage = null;
                ExceptionMessage = null;
            }
        }
    }
}
