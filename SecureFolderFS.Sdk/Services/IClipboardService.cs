namespace SecureFolderFS.Sdk.Services
{
    public interface IClipboardService
    {
        bool SetData<TData>(TData data);
    }
}
