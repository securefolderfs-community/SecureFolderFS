using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public partial class HealthIssueViewModel : ErrorViewModel, IWrapper<IResult>
    {
        [ObservableProperty] private string? _Icon; // TODO: Change to IImage
        [ObservableProperty] private SeverityType _Severity;

        /// <inheritdoc/>
        public IResult Inner { get; protected set; }

        public HealthIssueViewModel(IResult result, string title)
            : base(result)
        {
            ServiceProvider = DI.Default;
            Severity = SeverityType.Warning;
            Inner = result;
            Title = title;
        } 

        /// <inheritdoc/>
        protected override void UpdateStatus(IResult? result)
        {
            // Check if the result is null, in which case return since there always must be a result reported
            if (result is null)
                return;

            Inner = result;
            base.UpdateStatus(result);
        }

        [RelayCommand]
        protected virtual async Task CopyErrorAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsSupportedAsync())
                await ClipboardService.SetTextAsync(ExceptionMessage ?? Inner.GetExceptionMessage(), cancellationToken);
        }
    }
}
