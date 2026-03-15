using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Bindable(true)]
    public abstract partial class AuthenticationViewModel(string id) : ReportableViewModel, IAuthenticator, IDisposable
    {
        [ObservableProperty] private string? _Description;
        [ObservableProperty] private IImage? _Icon;

        /// <summary>
        /// Gets the unique ID of this authentication method.
        /// </summary>
        public string Id { get; } = id;

        /// <summary>
        /// Gets the value that indicates whether complementation is available for this authentication method.
        /// </summary>
        public abstract bool CanComplement { get; }

        /// <summary>
        /// Gets the stage (step) availability of the given authentication type.
        /// </summary>
        public abstract AuthenticationStage Availability { get; }

        /// <summary>
        /// Occurs when the user has provided credentials.
        /// </summary>
        public abstract event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        public override void Report(IResult? result)
        {
            _ = result;
        }

        /// <inheritdoc/>
        public abstract Task RevokeAsync(string? id, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default);

        [RelayCommand]
        protected abstract Task ProvideCredentialsAsync(CancellationToken cancellationToken);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
