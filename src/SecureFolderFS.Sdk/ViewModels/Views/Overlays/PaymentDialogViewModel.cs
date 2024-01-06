using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IIapService>]
    public sealed partial class PaymentDialogViewModel : DialogViewModel, IAsyncInitialize
    {
        public static PaymentDialogViewModel Instance { get; } = new();

        private PaymentDialogViewModel()
        {
            ServiceProvider = Ioc.Default;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (PrimaryButtonText is not null && PrimaryButtonText != "Buy")
                return;

            var price = await IapService.GetPriceAsync(IapProductType.SecureFolderFSPlus, cancellationToken);
            if (string.IsNullOrEmpty(price))
            {
                PrimaryButtonText = "Buy";
                PrimaryButtonEnabled = false;
            }
            else
                PrimaryButtonText = price;
        }
    }
}
