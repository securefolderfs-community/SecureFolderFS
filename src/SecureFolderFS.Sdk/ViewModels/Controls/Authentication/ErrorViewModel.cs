﻿using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Bindable(true)]
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
        public override void SetError(IResult? result)
        {
            ErrorMessage = result?.GetMessage();
        }
    }
}
