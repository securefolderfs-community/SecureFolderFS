using System.Security.Cryptography;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Prompts
{
    internal sealed class RecoveryPrompt : IOverlayControl
    {
        public RecoveryOverlayViewModel? ViewModel { get; private set; }
        
        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Result.Failure(null);
            
            var page = Shell.Current.CurrentPage;
            ViewModel.MasterKey = await page.DisplayPromptAsync("RecoverAccess".ToLocalized(), "EnterRecoveryKey".ToLocalized());
            
            var result = await ViewModel.RecoverAsync(default);
            if (!result)
                return Result.Failure(new CryptographicException(ViewModel.ErrorMessage));

            return Result.Success;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (RecoveryOverlayViewModel)viewable;
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            return Task.CompletedTask;
        }
    }
}
