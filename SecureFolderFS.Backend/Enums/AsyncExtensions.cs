namespace SecureFolderFS.Backend.Enums
{
    internal static class AsyncExtensions
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
