using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health
{
    [Inject<IClipboardService>]
    [Bindable(true)]
    public partial class HealthIssueViewModel : ErrorViewModel, IWrapper<IStorable>
    {
        [ObservableProperty] private string? _Icon; // TODO: Change to IImage
        [ObservableProperty] private SeverityType _Severity;

        /// <summary>
        /// Gets the <see cref="IResult"/> associated with this view model.
        /// </summary>
        protected IResult? Result { get; set; }

        /// <inheritdoc/>
        public IStorable Inner { get; }

        public HealthIssueViewModel(IStorable storable, IResult? result, string? title = null)
            : this(storable, title ?? "Unknown error")
        {
            Result = result;
        }

        public HealthIssueViewModel(IStorable storable, string title)
            : base(title)
        {
            ServiceProvider = DI.Default;
            Severity = SeverityType.Warning;
            Inner = storable;
            Title = title;
        }

        /// <inheritdoc/>
        protected override void UpdateStatus(IResult? result)
        {
            // Check if the result is null, in which case return since there always must be a result reported
            if (result is null)
                return;

            Result = result;
            base.UpdateStatus(result);
        }

        [RelayCommand]
        protected virtual async Task CopyErrorAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsSupportedAsync())
                await ClipboardService.SetTextAsync(ExceptionMessage
                                                    ?? Result?.GetExceptionMessage()
                                                    ?? ErrorMessage
                                                    ?? "Unknown error", cancellationToken);
        }

        [RelayCommand]
        protected virtual async Task CopyPathAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsSupportedAsync())
                await ClipboardService.SetTextAsync(Inner.Id, cancellationToken);
        }
    }
}
