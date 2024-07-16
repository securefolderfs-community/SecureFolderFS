﻿using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IIapService>]
    [Bindable(true)]
    public sealed partial class PaymentDialogViewModel : OverlayViewModel, IAsyncInitialize
    {
        public static PaymentDialogViewModel Instance { get; } = new();

        private PaymentDialogViewModel()
        {
            ServiceProvider = DI.Default;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Localize
            if (PrimaryButtonText is not null && PrimaryButtonText != "Buy")
                return;

            var price = await IapService.GetPriceAsync(IapProductType.SecureFolderFS_PlusSubscription, cancellationToken);
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
