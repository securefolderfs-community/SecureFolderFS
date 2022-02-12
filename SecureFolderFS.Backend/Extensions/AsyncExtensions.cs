namespace SecureFolderFS.Backend.Extensions
{
    internal static class AsyncExtensions // TODO: Move to SecureFolderFS.Shared
    {
        public static void RunAndForget(Action action)
        {
            try
            {
                _ = Task.Run(action).ConfigureAwait(false);
            }
            catch { }
        }
    }
}
