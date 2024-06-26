﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Bindable(true)]
    public abstract partial class AuthenticationViewModel(string id)
        : ReportableViewModel, IAuthenticator, IDisposable
    {
        [ObservableProperty] private string? _DisplayName;
        [ObservableProperty] private string? _Description;
        [ObservableProperty] private string? _Icon;

        /// <summary>
        /// Gets the unique ID of this authentication method.
        /// </summary>
        public string Id { get; } = id;

        /// <summary>
        /// Gets or sets the stage (step) availability of the given authentication type.
        /// </summary>
        public abstract AuthenticationType Availability { get; }

        /// <summary>
        /// Occurs when credentials have been provided by the user.
        /// </summary>
        public abstract event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        public override void SetError(IResult? result)
        {
            _ = result;
        }

        /// <inheritdoc/>
        public abstract Task RevokeAsync(string id, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IKey> SignAsync(string id, byte[]? data, CancellationToken cancellationToken = default);

        [RelayCommand]
        protected abstract Task ProvideCredentialsAsync(CancellationToken cancellationToken);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
