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
    [Inject<IIapService>]
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
        private async Task PurchaseLifetimeAsync(CancellationToken cancellationToken)
        {
            await IapService.PurchaseAsync(IapProductType.PlusSubscription, cancellationToken);
        }

        [RelayCommand]
        private async Task PurchaseSubscriptionAsync(CancellationToken cancellationToken)
        {
            await IapService.PurchaseAsync(IapProductType.PlusSubscription, cancellationToken);
        }
    }
}
