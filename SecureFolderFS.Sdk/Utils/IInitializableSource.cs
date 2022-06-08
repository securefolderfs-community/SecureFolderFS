namespace SecureFolderFS.Sdk.Utils
{
    internal interface IInitializableSource<TInitializeWith>
    {
        internal void Initialize(TInitializeWith param);
    }
}
