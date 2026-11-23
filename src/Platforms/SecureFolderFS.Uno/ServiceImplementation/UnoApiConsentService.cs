using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Api.Models;
using SecureFolderFS.Sdk.Api.Services;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using ApiConstants = SecureFolderFS.Sdk.Api.Constants;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IApiConsentService"/>
    internal sealed class UnoApiConsentService : IApiConsentService
    {
        /// <inheritdoc/>
        public async Task<ApiConsentResult> RequestConsentAsync(
            ApiConsentRequest request,
            CancellationToken cancellationToken = default)
        {
            var overlayService = DI.OptionalService<IOverlayService>();
            if (overlayService is null) // Unlikely
                return ApiConsentResult.Denied;

            var viewModel = new ApiConsentOverlayViewModel(
                request.ClientName,
                request.Evidence.ExecutablePath,
                request.Evidence.Signer,
                request.Evidence.IsVerified,
                DescribeScopes(request.RequestedScopes));

            var isGranted = false;
            await App.Instance?.MainWindowSynchronizationContext.PostOrExecuteAsync(async () =>
            {
                var result = await overlayService.ShowAsync(viewModel);
                isGranted = result.Positive();
            })!;
            
            return isGranted
                ? new ApiConsentResult(true, request.RequestedScopes)
                : ApiConsentResult.Denied;
        }
        
        private static IReadOnlyList<string> DescribeScopes(IReadOnlyList<string> scopes)
        {
            return scopes.Select(scope => scope switch
                {
                    ApiConstants.Scopes.VAULTS_READ => "ApiConsentScopeVaultsRead",
                    ApiConstants.Scopes.VAULTS_TRIGGER => "ApiConsentScopeVaultsTrigger",
                    ApiConstants.Scopes.APP_CONTROL => "ApiConsentScopeAppControl",
                    _ => null
                })
                .OfType<string>()
                .Select(key => $"•  {key.ToLocalized()}")
                .ToArray();
        }
    }
}
