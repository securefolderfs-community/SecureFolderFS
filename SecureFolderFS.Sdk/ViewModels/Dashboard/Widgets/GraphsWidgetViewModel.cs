using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.Sdk.ViewModels.Dashboard.Widgets
{
    public sealed class GraphsWidgetViewModel : BaseWidgetViewModel
    {
        public GraphControlViewModel ReadGraphViewModel { get; }

        public GraphControlViewModel WriteGraphViewModel { get; }

        public VaultIoSpeedReporterModel VaultIoSpeedReporterModel { get; }

        public GraphsWidgetViewModel()
        {
            ReadGraphViewModel = new();
            WriteGraphViewModel = new();
            VaultIoSpeedReporterModel = new(ReadGraphViewModel, WriteGraphViewModel)
            {
                ReadGraphModel = ReadGraphViewModel,
                WriteGraphModel = WriteGraphViewModel
            };
        }

        public void StartReporting()
        {
            VaultIoSpeedReporterModel.Start();
        }
    }
}
