using SecureFolderFS.Shared.Utils;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for printer status change events.
    /// </summary>
    public sealed class PrinterStatusChangedEventArgs : EventArgs
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
