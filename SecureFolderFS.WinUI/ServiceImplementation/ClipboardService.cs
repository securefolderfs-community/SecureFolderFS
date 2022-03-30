using SecureFolderFS.Backend.Services;
using System;
using Windows.ApplicationModel.DataTransfer;

#nullable enable

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class ClipboardService : IClipboardService
    {
        public bool SetData<TData>(TData data)
        {
            return data switch
            {
                string strData => InitializeAndSetDataToClipboard((dp) => dp.SetText(strData)),
                _ => false,
            };
        }

        private static bool InitializeAndSetDataToClipboard(Action<DataPackage> setDataCallback)
        {
            var dataPackage = new DataPackage();
            setDataCallback(dataPackage);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();

            return true;
        }
    }
}
