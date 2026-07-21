using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IIapService>, Inject<IOverlayService>]
    [Bindable(true)]
    public sealed partial class PaymentOverlayViewModel : OverlayViewModel, IAsyncInitialize
    {
        public static PaymentOverlayViewModel Instance { get; } = new();

        [ObservableProperty] private string? _LifetimeText;
        [ObservableProperty] private string? _SubscriptionText;

        private PaymentOverlayViewModel()
        {
            ServiceProvider = DI.Default;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (LifetimeText is not null && SubscriptionText is not null)
                return;

            var lifetime = await IapService.GetPriceAsync(IapProductType.PlusLifetime, cancellationToken);
            LifetimeText = "PlusAcquireLifetime".ToLocalized(lifetime ?? "{price}");

            var subscription = await IapService.GetPriceAsync(IapProductType.PlusSubscription, cancellationToken);
            SubscriptionText = "PlusAcquireSubscription".ToLocalized(subscription ?? "{price}");
        }

        [RelayCommand]
        private Task PurchaseLifetimeAsync(CancellationToken cancellationToken)
        {
            return PurchaseAsync(IapProductType.PlusLifetime, cancellationToken);
        }

        [RelayCommand]
        private Task PurchaseSubscriptionAsync(CancellationToken cancellationToken)
        {
            return PurchaseAsync(IapProductType.PlusSubscription, cancellationToken);
        }

        private async Task PurchaseAsync(IapProductType productType, CancellationToken cancellationToken)
        {
            // The Store surfaces its own native UI for progress, cancellation, and errors,
            // so we only need to react to a confirmed success here. On success, close the
            // overlay so the gated action that opened it can re-check ownership and proceed.
            var purchased = await IapService.PurchaseAsync(productType, cancellationToken);
            if (purchased)
                await OverlayService.CloseAllAsync();
        }
    }
}
