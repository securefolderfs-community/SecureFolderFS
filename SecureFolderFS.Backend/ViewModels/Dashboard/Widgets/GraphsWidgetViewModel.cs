using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Controls;

namespace SecureFolderFS.Backend.ViewModels.Dashboard.Widgets
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
