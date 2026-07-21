using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels.Authentication
{
    public sealed partial class AppPlatformLoginViewModel : AuthenticationViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;
        
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        public override bool CanComplement { get; } = false;

        /// <inheritdoc/>
        public override AuthenticationStage Availability { get; } = AuthenticationStage.FirstStageOnly;
        
        public AppPlatformLoginViewModel()
            : base(Core.Constants.Vault.Authentication.AUTH_APP_PLATFORM)
        {
        }

        /// <inheritdoc/>
        public override Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            return Task.FromException(new NotSupportedException());
        }

        /// <inheritdoc/>
        public override Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IResult<IKeyBytes>>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public override Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IResult<IKeyBytes>>(new NotSupportedException());
        }

        /// <inheritdoc/>
        protected override Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
