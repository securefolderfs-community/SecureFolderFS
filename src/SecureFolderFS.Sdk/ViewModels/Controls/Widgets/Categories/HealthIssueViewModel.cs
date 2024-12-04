using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories
{
    [Bindable(true)]
    public sealed partial class HealthIssueViewModel : ObservableObject, IViewable
    {
        private readonly IResult _result;
        private readonly IStorable _storable;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private string? _Message;
        [ObservableProperty] private SeverityType _Severity;

        public HealthIssueViewModel(IHealthResult healthResult)
            : this(healthResult, healthResult.Severity, healthResult.Value!)
        {
        }

        public HealthIssueViewModel(IResult result, SeverityType severity, IStorable storable)
        {
            _result = result;
            _storable = storable;
            Message = result.GetMessage();
            Severity = severity;
            Title = "Error".ToLocalized();
        }
    }
}
