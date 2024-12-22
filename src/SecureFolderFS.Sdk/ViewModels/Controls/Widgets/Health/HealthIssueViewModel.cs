using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories
{
    [Inject<IClipboardService>]
    [Bindable(true)]
    public sealed partial class HealthIssueViewModel : ErrorViewModel
    {
        private IResult _result;
        private readonly IStorable _storable;

        [ObservableProperty] private string? _Icon; // TODO: Change to IImage
        [ObservableProperty] private bool _CanResolve;
        [ObservableProperty] private SeverityType _Severity;

        public HealthIssueViewModel(IHealthResult healthResult)
            : this(healthResult, healthResult.Severity, healthResult.Value!)
        {
        }

        public HealthIssueViewModel(IResult result, SeverityType severity, IStorable storable)
            : base(result)
        {
            ServiceProvider = DI.Default;
            _result = result;
            _storable = storable;
            Severity = severity;
        }

        /// <inheritdoc/>
        protected override void UpdateStatus(IResult? result)
        {
            // Check if the result is null in which case return, since there always must be a result reported
            if (result is null)
                return;

            _result = result;
            Title = result switch
            {
                IHealthResult healthResult => healthResult.Title,
                _ => "Unknown error"
            };

            CanResolve = _result is IHealthResult;
            base.UpdateStatus(result);
        }

        [RelayCommand]
        private async Task ResolveAsync(CancellationToken cancellationToken)
        {
            if (_result is not IHealthResult healthResult)
                return;

            await healthResult.TryResolveAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task CopyErrorAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsSupportedAsync())
                await ClipboardService.SetTextAsync(ExceptionMessage ?? _result.GetExceptionMessage(), cancellationToken);
        }
    }
}
