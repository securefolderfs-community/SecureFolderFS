using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed class GraphsWidgetViewModel : BaseWidgetViewModel
    {
        public GraphControlViewModel ReadGraphViewModel { get; }

        public GraphControlViewModel WriteGraphViewModel { get; }

        public VaultLiveStatisticsModel VaultLiveStatisticsModel { get; }

        public GraphsWidgetViewModel(IWidgetModel widgetModel)
            : base(widgetModel)
        {
            ReadGraphViewModel = new();
            WriteGraphViewModel = new();
            VaultLiveStatisticsModel = new();
        }
        
        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {


            // We don't want to await it, since it's an async based timer
            _ = VaultLiveStatisticsModel.InitAsync(cancellationToken);
            await Task.CompletedTask;
        }
    }
}
