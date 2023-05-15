using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed partial class PaymentDialogViewModel : DialogViewModel, IAsyncInitialize
    {
        public static PaymentDialogViewModel Instance { get; } = new();

        private IIapService IapService { get; } = Ioc.Default.GetRequiredService<IIapService>();

        private PaymentDialogViewModel()
        {
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
