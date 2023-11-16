using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Printing;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utilities;
using SecureFolderFS.WinUI.Views.PrintPages;
using SecureFolderFS.WinUI.WindowViews;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using WinUIEx;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IPrinterService"/>
    internal sealed class WindowsPrinterService : IPrinterService
    {
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
                throw new NotSupportedException("Printing is not supported.");

            using var printer = new SimplePrinter();

            // Setup master key print page
            var printPage = new MasterKeyPrintPage();
            printPage.MasterKeyVaultNameText.Text = $"Master key for '{vaultName}'";
            printPage.MasterKeyText.Text = superSecret.ToString();

            await printer.PrintAsync(printPage);
        }
    }

    file sealed class SimplePrinter : IDisposable
    {
        private readonly TaskCompletionSource _tcs;
        private readonly PrintManager _printManager;
        private readonly PrintDocument _printDocument;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly IPrintDocumentSource _documentSource;
        private Page? _pageToPrint;

        public SimplePrinter()
        {
            _tcs = new TaskCompletionSource();
            _printDocument = new PrintDocument();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _documentSource = _printDocument.DocumentSource;
            _printDocument.Paginate += PrintDocument_Paginate;               // Creates page previews for documents
            _printDocument.GetPreviewPage += PrintDocument_GetPreviewPage;   // Creates a specific page preview
            _printDocument.AddPages += PrintDocument_AddPages;               // Provides all pages to be printed
            
            _printManager = PrintManagerInterop.GetForWindow(MainWindow.Instance.GetWindowHandle());
            _printManager.PrintTaskRequested += PrintManager_PrintTaskRequested;
        }

        public async Task PrintAsync(Page page)
        {
            _pageToPrint = page;
            await PrintManagerInterop.ShowPrintUIForWindowAsync(MainWindow.Instance.GetWindowHandle());
            await _tcs.Task;
        }

        private void PrintManager_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            PrintTask printTask;
            _dispatcherQueue.EnqueueAsync(() =>
            {
                printTask = args.Request.CreatePrintTask("Print", e =>
                {
                    // Set the document source
                    e.SetSource(_documentSource);
                });

                // Handle PrintTask.Completed to catch failed print jobs
                printTask.Completed += PrintTask_Completed;
            }).Wait();

            void PrintTask_Completed(PrintTask s, PrintTaskCompletedEventArgs e)
            {
                printTask.Completed -= PrintTask_Completed;

                // Notify the user when the print operation fails.
                if (e.Completion == PrintTaskCompletion.Failed)
                    _tcs.TrySetException(new Exception("Printing operation failed."));
                else
                    _tcs.TrySetResult();
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
            _printDocument.Paginate -= PrintDocument_Paginate;
            _printDocument.GetPreviewPage -= PrintDocument_GetPreviewPage;
            _printDocument.AddPages -= PrintDocument_AddPages;
            _printManager.PrintTaskRequested -= PrintManager_PrintTaskRequested;
        }
    }

    /// <summary>
    /// Event arguments for printer status change events.
    /// </summary>
    file sealed class PrinterStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the status result of the printer.
        /// </summary>
        public IResult Status { get; }

        public PrinterStatusChangedEventArgs(IResult status)
        {
            Status = status;
        }
    }
}
