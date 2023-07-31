using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Printing;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.WinUI.Views.PrintPages;
using SecureFolderFS.WinUI.WindowViews;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using CommunityToolkit.WinUI;
using WinUIEx;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IPrinterService"/>
    internal sealed class PrinterService : IPrinterService
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly PrintManager _printManager;
        private readonly PrintDocument _printDocument;
        private readonly IPrintDocumentSource _printDocumentSource;
        private Page? _pageToPrint;

        /// <inheritdoc/>
        public event EventHandler<EventArgs>? StateChanged;

        public PrinterService()
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _printDocument = new PrintDocument();
            _printDocumentSource = _printDocument.DocumentSource;
            _printDocument.Paginate += PrintDocument_Paginate;               // Creates page previews for documents
            _printDocument.GetPreviewPage += PrintDocument_GetPreviewPage;   // Creates a specific page preview
            _printDocument.AddPages += PrintDocument_AddPages;               // Provides all pages to be printed
            
            _printManager = PrintManagerInterop.GetForWindow(MainWindow.Instance.GetWindowHandle());
            _printManager.PrintTaskRequested += PrintManager_PrintTaskRequested;
        }

        /// <inheritdoc/>
        public Task<bool> IsSupportedAsync()
        {
            var supported = PrintManager.IsSupported();
            return Task.FromResult(supported);
        }

        /// <inheritdoc/>
        public async Task PrintMasterKeyAsync(IDisposable superSecret, string vaultName)
        {
            if (!await IsSupportedAsync())
                throw new NotSupportedException("Printing is not supported");

            // Setup master key print page
            var printPage = new MasterKeyPrintPage();
            printPage.MasterKeyVaultNameText.Text = $"Master key for {vaultName}";
            printPage.MasterKeyText.Text = superSecret.ToString();

            _pageToPrint = printPage;
            await PrintManagerInterop.ShowPrintUIForWindowAsync(MainWindow.Instance.GetWindowHandle());
        }

        private void PrintManager_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            PrintTask printTask;
            _dispatcherQueue.EnqueueAsync(() =>
            {
                printTask = args.Request.CreatePrintTask("Print", e =>
                {
                    // Set the document source
                    e.SetSource(_printDocumentSource);
                });

                // Handle PrintTask.Completed to catch failed print jobs
                printTask.Completed += PrintTask_Completed;
            }).Wait();

            void PrintTask_Completed(PrintTask s, PrintTaskCompletedEventArgs e)
            {
                printTask.Completed -= PrintTask_Completed;

                // Notify the user when the print operation fails.
                if (e.Completion == PrintTaskCompletion.Failed)
                    StateChanged?.Invoke(this, new PrinterStatusChangedEventArgs(new CommonResult(false)));
            }
        }

        private void PrintDocument_AddPages(object sender, AddPagesEventArgs e)
        {
            _printDocument.AddPage(_pageToPrint);
            _printDocument.AddPagesComplete();
        }

        private void PrintDocument_GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            _printDocument.SetPreviewPage(e.PageNumber, _pageToPrint);
        }

        private void PrintDocument_Paginate(object sender, PaginateEventArgs e)
        {
            _printDocument.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
